using ParserUtils;

using TPLangParser.TPLang.Instructions;

namespace TPLangParser.TPLang.SymbolTable;

// Walks a parsed TP program and records every register and IO port usage.
//
// TP programs never *declare* symbols — registers (R/PR/SR/AR) and IO ports
// (D/R/S/U-I/O, A/G/W-I/O) only ever get read or written. So the builder visits
// every instruction, classifies each register/port occurrence as a Read or a
// Write from its syntactic position, and records it. An occurrence is a Write
// only when it is the assignment target on the left of an '='; everything else
// (operands, conditions, motion targets, and *every* index expression — even on
// the left-hand side, e.g. the inner R[2] of R[R[2]]=...) is a Read.
public static class TpSymbolTableBuilder
{
    private const TpSymbolRefKind Read = TpSymbolRefKind.Read;
    private const TpSymbolRefKind Write = TpSymbolRefKind.Write;

    public static TpSymbolTable Build(TpProgram program)
    {
        var table = new TpSymbolTable();
        foreach (var instruction in program.Main.Instructions)
        {
            TraverseInstruction(instruction, table);
        }

        return table;
    }

    private static void TraverseInstruction(TpInstruction instruction, TpSymbolTable table)
    {
        switch (instruction)
        {
            // ---- Motion -----------------------------------------------------
            case TpMotionInstruction i:
                foreach (var position in i.Positions)
                {
                    RecordRegister(position, Read, table);
                }
                TraverseMotionSpeed(i.Speed, table);
                foreach (var option in i.Options)
                {
                    TraverseMotionOption(option, table);
                }
                break;

            // ---- Register assignment ---------------------------------------
            case TpRegisterAssignment i:
                RecordRegister(i.Register, Write, table);
                TraverseArithmetic(i.Expression, table);
                break;

            // ---- Position register -----------------------------------------
            case TpPosRegAssignmentInstruction i:
                RecordRegister(i.PosReg, Write, table);
                TraversePosRegExpression(i.Expr, table);
                break;
            case TpPosRegElementAssignmentInstruction i:
                RecordRegister(i.PosReg, Write, table);
                TraverseArithmetic(i.Expr, table);
                break;

            // ---- String register -------------------------------------------
            case TpStringRegisterAssignment i:
                RecordRegister(i.StringRegister, Write, table);
                RecordRegister(i.Value.Register, Read, table);
                break;
            case TpStringRegisterConcatenation i:
                RecordRegister(i.StringRegister, Write, table);
                RecordRegister(i.Lhs.Register, Read, table);
                RecordRegister(i.Rhs.Register, Read, table);
                break;
            case TpStringRegisterLength i:
                RecordRegister(i.ResultRegister, Write, table);
                RecordRegister(i.StringRegister, Read, table);
                break;
            case TpStringRegisterSearch i:
                RecordRegister(i.ResultRegister, Write, table);
                RecordRegister(i.InputString.Register, Read, table);
                RecordRegister(i.SearchString.Register, Read, table);
                break;
            case TpStringRegisterCut i:
                RecordRegister(i.ResultRegister, Write, table);
                RecordRegister(i.InputString.Register, Read, table);
                TraverseValue(i.BeginIndex, Read, table);
                TraverseValue(i.EndIndex, Read, table);
                break;

            // ---- IO ---------------------------------------------------------
            case TpDigitalIOInstruction i:
                TraverseValue(i.Lhs, Write, table);
                TraverseValue(i.Rhs, Read, table);
                break;
            case TpRobotIOInstruction i:
                TraverseValue(i.Lhs, Write, table);
                TraverseValue(i.Rhs, Read, table);
                break;
            case TpAnalogIOInstruction i:
                TraverseValue(i.Lhs, Write, table);
                TraverseValue(i.Rhs, Read, table);
                break;
            case TpGroupIOInstruction i:
                TraverseValue(i.Lhs, Write, table);
                TraverseValue(i.Rhs, Read, table);
                break;
            case TpWeldingIOInstruction i:
                TraverseValue(i.Lhs, Write, table);
                TraverseValue(i.Rhs, Read, table);
                break;

            // ---- Math -------------------------------------------------------
            case TpMathInstruction i:
                TraverseValue(i.Variable, Write, table);
                TraverseMathExpression(i.Expression, table);
                break;

            // ---- Mixed logic ------------------------------------------------
            case TpMixedLogicAssignment i:
                TraverseValue(i.Assignable, Write, table);
                TraverseMixedLogic(i.Expression, table);
                break;
            case TpMixedLogicWaitInstruction i:
                TraverseMixedLogic(i.Expression, table);
                break;

            // ---- Branching --------------------------------------------------
            case TpIfInstruction i:
                TraverseIfExpression(i.Expression, table);
                TraverseBranchingAction(i.Action, table);
                break;
            case TpIfThenInstruction i:
                TraverseMixedLogic(i.Expression, table);
                break;
            case TpSelectInstruction i:
                RecordRegister(i.Register, Read, table);
                TraverseSelectCase(i.Case, table);
                break;
            case TpSelectCaseInstruction i:
                TraverseSelectCase(i.Case, table);
                break;
            case TpLabelDefinitionInstruction i:
                TraverseLabel(i.Label, table);
                break;
            // JMP and CALL are branching actions that also stand alone.
            case TpBranchingAction i:
                TraverseBranchingAction(i, table);
                break;

            // ---- Conditions (wait / skip) ----------------------------------
            case TpWaitTime i:
                TraverseValue(i.WaitTime, Read, table);
                break;
            case TpWaitCondition i:
                TraverseLogicExpression(i.Condition, table);
                break;
            case TpSkipCondition i:
                TraverseLogicExpression(i.Condition, table);
                break;

            // ---- For loop ---------------------------------------------------
            case TpBeginForInstruction i:
                RecordRegister(i.Counter, Write, table);
                TraverseValue(i.InitialValue, Read, table);
                TraverseValue(i.TargetValue, Read, table);
                break;

            // ---- Offset / frames -------------------------------------------
            case TpOffsetConditionInstruction i:
                RecordRegister(i.PositionRegister, Read, table);
                if (i.UserFrame is { } offsetFrame)
                {
                    TraverseAccess(offsetFrame.Access, table);
                }
                break;
            case TpUserFrameUseInstruction i:
                TraverseValue(i.Value, Read, table);
                break;
            case TpUserToolUseInstruction i:
                TraverseValue(i.Value, Read, table);
                break;
            case TpUserFrameSetInstruction i:
                TraverseAccess(i.UserFrame.Access, table);
                RecordRegister(i.Value, Read, table);
                break;
            case TpUserToolSetInstruction i:
                TraverseAccess(i.UserTool.Access, table);
                RecordRegister(i.Value, Read, table);
                break;

            // ---- Misc -------------------------------------------------------
            case TpOverrideIndirect i:
                RecordRegister(i.Register.Register, Read, table);
                break;
            case TpParameterWriteInstruction i:
                TraverseValue(i.Parameter, Write, table);
                TraverseValue(i.Value, Read, table);
                break;
            case TpParameterReadInstruction i:
                RecordRegister(i.Register, Write, table);
                TraverseValue(i.Parameter, Read, table);
                break;
            case TpJointMaxSpeedInstruction i:
                TraverseAccess(i.Access, table);
                TraverseValue(i.Value, Read, table);
                break;
            case TpLinearMaxSpeedInstruction i:
                TraverseAccess(i.Access, table);
                TraverseValue(i.Value, Read, table);
                break;
            case TpRsrInstruction i:
                TraverseAccess(i.Access, table);
                break;
            case TpUserAlarmInstruction i:
                TraverseAccess(i.Access, table);
                break;
            case TpTimerInstruction i:
                TraverseAccess(i.Access, table);
                break;
            case TpPayloadInstruction i:
                TraverseAccess(i.Access, table);
                break;

            // ---- Welding ----------------------------------------------------
            case TpWeldInstruction i:
                TraverseWeldInstructionArgs(i.Args, table);
                break;
            case TpWeaveStartInstruction i:
                TraverseWeldInstructionArgs(i.Args, table);
                break;
            case TpWeaveEndInstruction i:
                if (i.Schedule is { } schedule)
                {
                    TraverseWeldInstructionArgs(schedule, table);
                }
                break;

            // Everything else (labels' siblings, program-control, macros, run,
            // collision, monitor, comments, ...) carries no register/IO usage.
            default:
                break;
        }
    }

    // ---- Branching helpers --------------------------------------------------

    private static void TraverseBranchingAction(TpBranchingAction action, TpSymbolTable table)
    {
        switch (action)
        {
            case TpJumpLabelInstruction a:
                TraverseLabel(a.Label, table);
                break;
            case TpCallInstruction a:
                TraverseCallMethod(a.CallMethod, table);
                foreach (var argument in a.Arguments)
                {
                    TraverseValue(argument, Read, table);
                }
                break;
            case TpMixedLogicAssignmentBranchingAction a:
                TraverseInstruction(a.Instruction, table);
                break;
            case TpPulseBranchingAction a:
                RecordPort(a.IoPort.IOPort, Write, table);
                break;
        }
    }

    private static void TraverseCallMethod(TpCallMethod method, TpSymbolTable table)
    {
        if (method is TpCallByStringRegister call)
        {
            RecordRegister(call.StringRegister, Read, table);
        }
    }

    private static void TraverseSelectCase(TpSelectCase selectCase, TpSymbolTable table)
    {
        // The else-case carries a null comparison value.
        if (selectCase.Value is { } value)
        {
            TraverseValue(value, Read, table);
        }

        TraverseBranchingAction(selectCase.Action, table);
    }

    private static void TraverseLabel(TpLabel label, TpSymbolTable table)
        => TraverseAccess(label.LabelNumber, table);

    // ---- Expression helpers -------------------------------------------------

    private static void TraverseIfExpression(TpIfExpression expression, TpSymbolTable table)
    {
        switch (expression)
        {
            case TpIfExpressionLogic e:
                TraverseLogicExpression(e.Expression, table);
                break;
            case TpIfExpressionMixedLogic e:
                TraverseMixedLogic(e.Expression, table);
                break;
        }
    }

    private static void TraverseLogicExpression(TpLogicExpression expression, TpSymbolTable table)
    {
        switch (expression)
        {
            case TpLogicExpressionSingle e:
                TraverseComparison(e.Expression, table);
                break;
            case TpLogicExpressionAnd e:
                foreach (var comparison in e.Expression)
                {
                    TraverseComparison(comparison, table);
                }
                break;
            case TpLogicExpressionOr e:
                foreach (var comparison in e.Expression)
                {
                    TraverseComparison(comparison, table);
                }
                break;
        }
    }

    private static void TraverseComparison(TpComparisonExpression comparison, TpSymbolTable table)
    {
        TraverseValue(comparison.Lhs, Read, table);
        TraverseValue(comparison.Rhs, Read, table);
    }

    private static void TraverseMixedLogic(TpMixedLogicExpression expression, TpSymbolTable table)
    {
        switch (expression)
        {
            case TpMixedLogicValue e:
                TraverseValue(e.Value, Read, table);
                break;
            case TpMixedLogicUnaryNot e:
                TraverseMixedLogic(e.Term, table);
                break;
            case TpMixedLogicBinary e:
                TraverseMixedLogic(e.Lhs, table);
                TraverseMixedLogic(e.Rhs, table);
                break;
        }
    }

    private static void TraverseArithmetic(TpArithmeticExpression expression, TpSymbolTable table)
    {
        switch (expression)
        {
            case TpArithmeticBinary e:
                TraverseValue(e.Lhs.Value, Read, table);
                TraverseArithmetic(e.Rhs, table);
                break;
            case TpArithmeticValue e:
                TraverseValue(e.Value, Read, table);
                break;
        }
    }

    private static void TraversePosRegExpression(TpPosRegExpression expression, TpSymbolTable table)
    {
        switch (expression)
        {
            case TpPosRegBinary e:
                TraverseValue(e.Lhs.Value, Read, table);
                TraversePosRegExpression(e.Rhs, table);
                break;
            case TpPosRegValue e:
                TraverseValue(e.Value, Read, table);
                break;
        }
    }

    private static void TraverseMathExpression(TpMathExpression expression, TpSymbolTable table)
    {
        switch (expression)
        {
            case TpAtan2Expression e:
                RecordRegister(e.Value1, Read, table);
                RecordRegister(e.Value2, Read, table);
                break;
            case TpSqrtExpression e: RecordRegister(e.Value, Read, table); break;
            case TpSinExpression e: RecordRegister(e.Value, Read, table); break;
            case TpCosExpression e: RecordRegister(e.Value, Read, table); break;
            case TpTanExpression e: RecordRegister(e.Value, Read, table); break;
            case TpAsinExpression e: RecordRegister(e.Value, Read, table); break;
            case TpAcosExpression e: RecordRegister(e.Value, Read, table); break;
            case TpAtanExpression e: RecordRegister(e.Value, Read, table); break;
            case TpLnExpression e: RecordRegister(e.Value, Read, table); break;
            case TpExpExpression e: RecordRegister(e.Value, Read, table); break;
            case TpAbsExpression e: RecordRegister(e.Value, Read, table); break;
            case TpTruncExpression e: RecordRegister(e.Value, Read, table); break;
            case TpRoundExpression e: RecordRegister(e.Value, Read, table); break;
        }
    }

    // ---- Motion helpers -----------------------------------------------------

    private static void TraverseMotionSpeed(TpMotionSpeed speed, TpSymbolTable table)
    {
        if (speed is TpMotionSpeedIndirect indirect)
        {
            RecordRegister(indirect.Register, Read, table);
        }
    }

    private static void TraverseMotionOption(TpMotionOption option, TpSymbolTable table)
    {
        switch (option)
        {
            case TpAlimOptionRegister o:
                RecordRegister(o.Register, Read, table);
                break;
            case TpLinearDistanceOptionRegister o:
                RecordRegister(o.Register, Read, table);
                break;
            case TpOffsetOption o when o.PositionRegister is { } pr:
                RecordRegister(pr, Read, table);
                break;
            case TpToolOffsetOption o when o.PositionRegister is { } pr:
                RecordRegister(pr, Read, table);
                break;
            case TpTorchAngleOption o when o.PositionRegister is { } pr:
                RecordRegister(pr, Read, table);
                break;
            case TpWeldOption o:
                TraverseWeldOptionArguments(o.Args, table);
                break;
            case TpSkipJumpOption o:
                TraverseLabel(o.Label, table);
                break;
            case TpSkipOption o:
                TraverseLabel(o.Label, table);
                if (o.Assignment is { } assignment)
                {
                    TraverseInstruction(assignment, table);
                }
                break;
        }
    }

    private static void TraverseWeldOptionArguments(TpWeldOptionArguments arguments, TpSymbolTable table)
    {
        if (arguments is TpWeldOptionProcedures procedures)
        {
            TraverseWeldOptionArg(procedures.Procedure, table);
            TraverseWeldOptionArg(procedures.Schedule, table);
        }
    }

    private static void TraverseWeldOptionArg(TpWeldOptionArg argument, TpSymbolTable table)
    {
        if (argument is TpWeldOptionRegisterArg register)
        {
            RecordRegister(register.Register, Read, table);
        }
    }

    private static void TraverseWeldInstructionArgs(TpWeldInstructionArgs arguments, TpSymbolTable table)
    {
        switch (arguments)
        {
            case TpWeldInstructionRegister a:
                RecordRegister(a.Register, Read, table);
                break;
            case TpWeldInstructionWeldSchedule a:
                TraverseAccess(a.Access, table);
                break;
        }
    }

    // ---- Value / access dispatch -------------------------------------------

    private static void TraverseValue(TpValue value, TpSymbolRefKind refKind, TpSymbolTable table)
    {
        switch (value)
        {
            case TpValueRegister v:
                RecordRegister(v.Register, refKind, table);
                break;
            case TpValuePosition v:
                RecordRegister(v.Pos, refKind, table);
                break;
            case TpValueIOPort v:
                RecordPort(v.IOPort, refKind, table);
                break;
            case TpValueOnOffIOPort v:
                RecordPort(v.IOPort, refKind, table);
                break;
            case TpValueNumericalIOPort v:
                RecordPort(v.IOPort, refKind, table);
                break;
            case TpValueMathExpr v:
                TraverseMathExpression(v.Expression, table);
                break;
            case TpValueFlag v:
                RecordFlag(v.Flag, refKind, table);
                break;
            // Timers and frames are not tracked symbols, but their (possibly
            // indirect) index still references registers.
            case TpValueTimer v:
                TraverseAccess(v.Access, table);
                break;
            case TpValueUFrame v:
                TraverseAccess(v.UserFrame.Access, table);
                break;
            case TpValueUTool v:
                TraverseAccess(v.UserFrame.Access, table);
                break;
            case TpValueSystemVariable v:
                table.RecordNamedUsage(TpSymbolKind.SysVar, v.Variable, refKind, v.Start, v);
                break;
            case TpValueKarelVariable v:
                table.RecordNamedUsage(TpSymbolKind.KarelVar, TpSymbolTable.KarelVariableName(v), refKind, v.Start, v);
                break;
            // Constants, IO states, strings, pulses, LPOS/JPOS and ERR_NUM
            // carry no register or IO port usage.
            default:
                break;
        }
    }

    // Records nested register usages found inside an index expression. Every
    // index is a read, even when it indexes the target of a write.
    private static void TraverseAccess(TpAccess access, TpSymbolTable table)
    {
        switch (access)
        {
            case TpAccessIndirect a:
                RecordRegister(a.Register, Read, table);
                break;
            case TpAccessMultiple a:
                TraverseValue(a.Number, Read, table);
                TraverseValue(a.Item, Read, table);
                break;
        }
    }

    // ---- Recording ----------------------------------------------------------

    private static void RecordRegister(TpGenericRegister register, TpSymbolRefKind refKind, TpSymbolTable table)
    {
        // Plain program positions (P[n]) are not registers — only a position
        // register (PR) is. An indirectly indexed register (R[R[2]], PR[R[3]])
        // can't be resolved to a concrete symbol statically, so the outer
        // register is not recorded — only its inner index register (a read) is.
        if (TpSymbolTable.TryResolveKey(register, out var kind, out var index))
        {
            table.RecordIndexedUsage(kind, index, refKind, register.Start, register);
        }

        TraverseAccess(register.Access, table);
    }

    private static void RecordPort(TpIOPort port, TpSymbolRefKind refKind, TpSymbolTable table)
    {
        // As with registers, an indirectly indexed port can't be resolved.
        if (TpSymbolTable.TryResolveKey(port, out var kind, out var index))
        {
            table.RecordIndexedUsage(kind, index, refKind, port.Start, port);
        }

        TraverseAccess(port.PortNumber, table);
    }

    private static void RecordFlag(TpFlag flag, TpSymbolRefKind refKind, TpSymbolTable table)
    {
        // As with registers, an indirectly indexed flag (F[R[2]]) can't be
        // resolved to a concrete symbol — only its inner index register (a read).
        if (TpSymbolTable.TryResolveKey(flag, out var index))
        {
            table.RecordIndexedUsage(TpSymbolKind.Flag, index, refKind, flag.Start, flag);
        }

        TraverseAccess(flag.Access, table);
    }
}