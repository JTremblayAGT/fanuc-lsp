using KarelParser.Conditions;
using KarelParser.Instructions;

namespace KarelParser.SymbolTable;

public static class KarelSymbolTableBuilder
{
    public static KarelSymbolTable Build(KarelProgram program)
    {
        var table = new KarelSymbolTable
        {
            ScopeStart = program.Start,
            ScopeEnd = program.End
        };
        foreach (var decl in program.Declarations)
        {
            TraverseDeclaration(decl, table);
        }
        foreach (var routine in program.Routines)
        {
            TraverseRoutine(routine, table);
        }
        foreach (var stmt in program.Statements)
        {
            TraverseStatement(stmt, table);
        }

        RegisterQualifiedSymbols(program, table);
        return table;
    }

    // Builds the fully-qualified path index: every program-level variable, plus
    // each struct field reachable through it, addressed as "Var.Field1.Field2".
    // These are exactly the names a TP program can reference as
    // $[PROG]Var.Field1.Field2, so the index lets a Karel symbol find its
    // references across TP files. Per-TYPE field symbols (in the lexical table)
    // are left untouched; this is an additional, variable-rooted view.
    private static void RegisterQualifiedSymbols(KarelProgram program, KarelSymbolTable table)
    {
        // Resolve named struct types the same way the TP completion provider
        // does. First declaration of a given name wins.
        var structures = program.Declarations
            .OfType<KarelTypeDeclaration>()
            .SelectMany(decl => decl.Type)
            .Where(type => type.Type is KarelStructure)
            .GroupBy(type => type.Identifier, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => (KarelStructure)group.First().Type, StringComparer.OrdinalIgnoreCase);

        foreach (var variable in program.Declarations
            .OfType<KarelVariableDeclaration>()
            .SelectMany(decl => decl.Variable))
        {
            // The variable itself is addressable as $[PROG]Var.
            table.AddQualifiedSymbol(variable.Identifier, variable.Identifier, KarelSymbolKind.Variable, variable.Type, variable.Start);
            ExpandFields(variable.Identifier, variable.Type, structures, table,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }
    }

    // Recursively registers the fields of a struct-typed path. `visiting` guards
    // against self-referential types looping forever along a single path.
    private static void ExpandFields(
        string pathPrefix,
        KarelDataType type,
        Dictionary<string, KarelStructure> structures,
        KarelSymbolTable table,
        HashSet<string> visiting)
    {
        var (structure, typeName) = ResolveStructure(type, structures);
        if (structure is null || !visiting.Add(typeName))
        {
            return;
        }

        foreach (var field in structure.Fields)
        {
            var fullName = $"{pathPrefix}.{field.Identifier}";
            table.AddQualifiedSymbol(fullName, field.Identifier, KarelSymbolKind.StructField, field.Type, field.Start);
            ExpandFields(fullName, field.Type, structures, table, visiting);
        }

        visiting.Remove(typeName);
    }

    // Resolves a data type (unwrapping arrays) to the named structure it refers
    // to, or null when it isn't a known struct type.
    private static (KarelStructure? Structure, string TypeName) ResolveStructure(
        KarelDataType type,
        Dictionary<string, KarelStructure> structures)
        => type switch
        {
            KarelTypeName name when structures.TryGetValue(name.Identifier, out var structure)
                => (structure, name.Identifier),
            KarelTypeArray array => ResolveStructure(array.Type, structures),
            _ => (null, string.Empty)
        };

    private static void TraverseDeclaration(KarelDeclaration decl, KarelSymbolTable table)
    {
        switch (decl)
        {
            case KarelVariableDeclaration d:
                foreach (var v in d.Variable)
                {
                    table.AddSymbol(v.Identifier, KarelSymbolKind.Variable, v.Type, v.Start);
                    if (v.Type is KarelTypeName typeName)
                    {
                        table.AddReference(typeName.Identifier, typeName.Start);
                    }
                }
                break;
            case KarelConstantDeclaration d:
                foreach (var c in d.Constants)
                {
                    table.AddSymbol(c.Identifier, KarelSymbolKind.Constant, new KarelTypeName("INTEGER", 0), c.Start);
                }
                break;
            case KarelTypeDeclaration d:
                foreach (var t in d.Type)
                {
                    table.AddSymbol(t.Identifier, KarelSymbolKind.Type, t.Type, t.Start);
                    TraverseUserType(t.Type, table);
                }
                break;
        }
    }

    private static void TraverseUserType(KarelUserType userType, KarelSymbolTable table)
    {
        switch (userType)
        {
            case KarelStructure s:
                foreach (var field in s.Fields)
                {
                    table.AddSymbol(field.Identifier, KarelSymbolKind.StructField, field.Type, field.Start);
                    if (field.Type is KarelTypeName typeName)
                    {
                        table.AddReference(typeName.Identifier, typeName.Start);
                    }
                }
                break;
            case KarelDataType _:
                break;
        }
    }

    private static void TraverseRoutine(KarelRoutine routine, KarelSymbolTable table)
    {
        table.AddSymbol(routine.Identifier, KarelSymbolKind.Routine, routine.ReturnType, routine.Start);
        var scope = table.CreateRoutine(routine.Start, routine.End);
        foreach (var arg in routine.Args)
        {
            scope.AddSymbol(arg.Identifier, KarelSymbolKind.Variable, arg.Type, arg.Start);
        }
        switch (routine.Body)
        {
            case KarelRoutineBody body:
                foreach (var decl in body.Locals)
                {
                    TraverseDeclaration(decl, scope);
                }
                foreach (var stmt in body.Body)
                {
                    TraverseStatement(stmt, scope);
                }
                break;
            case KarelFromBody _:
                break;
        }
    }

    private static void TraverseStatement(KarelStatement stmt, KarelSymbolTable table)
    {
        switch (stmt)
        {
            case KarelAssignment s:
                TraverseExpression(s.Variable, table);
                TraverseExpression(s.Expr, table);
                break;
            case KarelCall s:
                table.AddReference(s.Identifier, s.Start);
                foreach (var arg in s.Args)
                {
                    TraverseExpression(arg, table);
                }
                break;
            case KarelIfThenElse s:
                TraverseExpression(s.Expr, table);
                foreach (var s2 in s.Body)
                {
                    TraverseStatement(s2, table);
                }

                foreach (var s2 in s.Else)
                {
                    TraverseStatement(s2, table);
                }
                break;
            case KarelIfThen s:
                TraverseExpression(s.Expr, table);
                foreach (var s2 in s.Body)
                {
                    TraverseStatement(s2, table);
                }
                break;
            case KarelFor s:
                TraverseExpression(s.InitialValue, table);
                TraverseExpression(s.TargetValue, table);
                foreach (var s2 in s.Body)
                {
                    TraverseStatement(s2, table);
                }
                break;
            case KarelRepeat s:
                foreach (var s2 in s.Body)
                {
                    TraverseStatement(s2, table);
                }
                TraverseExpression(s.Expr, table);
                break;
            case KarelWhile s:
                TraverseExpression(s.Expr, table);
                foreach (var s2 in s.Body)
                {
                    TraverseStatement(s2, table);
                }
                break;
            case KarelSelect s:
                TraverseExpression(s.Expr, table);
                foreach (var c in s.Cases)
                {
                    TraverseCase(c, table);
                }
                if (s.ElseCase != null)
                {
                    TraverseCase(s.ElseCase, table);
                }
                break;
            case KarelUsing s:
                foreach (var v in s.Variables)
                {
                    TraverseExpression(v, table);
                }
                foreach (var s2 in s.Body)
                {
                    TraverseStatement(s2, table);
                }
                break;
            case KarelReturn s:
                if (s.Expr != null)
                {
                    TraverseExpression(s.Expr, table);
                }
                break;
            case KarelPause s:
                if (s.TaskNumber != null)
                {
                    TraverseExpression(s.TaskNumber, table);
                }
                break;
            case KarelDelay s:
                TraverseExpression(s.Expr, table);
                break;
            case KarelSignal s:
                TraverseExpression(s.Number, table);
                break;
            case KarelPulse s:
                TraverseExpression(s.Index, table);
                TraverseExpression(s.Time, table);
                break;
            case KarelWait s:
                TraverseCondition(s.Condition, table);
                break;
            case KarelConnectTimer s:
                table.AddReference(s.Identifier, s.Start);
                break;
            case KarelDisconnectTimer s:
                table.AddReference(s.Identifier, s.Start);
                break;
            case KarelCondition s:
                TraverseExpression(s.HandlerNumber, table);
                if (s.With != null)
                {
                    foreach (var a in s.With.Assignments)
                    {
                        TraverseExpression(a.Expr, table);
                    }
                }
                foreach (var w in s.When)
                {
                    TraverseWhen(w, table);
                }
                break;
            case KarelEnable s:
                TraverseExpression(s.Expr, table);
                break;
            case KarelDisable s:
                TraverseExpression(s.Expr, table);
                break;
            case KarelPurge s:
                TraverseExpression(s.Expr, table);
                break;
            case KarelRead s:
                if (s.Variable != null)
                {
                    TraverseExpression(s.Variable, table);
                }
                foreach (var item in s.Items)
                {
                    TraverseItem(item, table);
                }
                break;
            case KarelWrite s:
                if (s.Variable != null)
                {
                    TraverseExpression(s.Variable, table);
                }
                foreach (var item in s.Items)
                {
                    TraverseItem(item, table);
                }
                break;
            case KarelOpenFile s:
                TraverseExpression(s.File, table);
                TraverseExpression(s.Usage, table);
                TraverseExpression(s.Spec, table);
                break;
            case KarelOpenHand s:
                TraverseExpression(s.Hand, table);
                break;
            case KarelCloseFile s:
                TraverseExpression(s.File, table);
                break;
            case KarelCloseHand s:
                TraverseExpression(s.Hand, table);
                break;
            case KarelRelaxHand s:
                TraverseExpression(s.Hand, table);
                break;
            case KarelCancelFile s:
                TraverseExpression(s.File, table);
                break;
            case KarelAbort _:
            case KarelCancel _:
            case KarelHold _:
            case KarelUnhold _:
            case KarelStop _:
            case KarelResume _:
            case KarelRelease _:
            case KarelAttach _:
            case KarelGoto _:
            case KarelLabel _:
                break;
        }
    }

    private static void TraverseExpression(KarelExpression expr, KarelSymbolTable table)
    {
        switch (expr)
        {
            case KarelIdentifier id:
                table.AddReference(id.Identifier, id.Start);
                break;
            case KarelFieldAccess fa:
                TraverseExpression(fa.Variable, table);
                break;
            case KarelArrayAccess aa:
                TraverseExpression(aa.Variable, table);
                foreach (var idx in aa.Indices)
                {
                    TraverseExpression(idx, table);
                }
                break;
            case KarelPathAccess pa:
                TraverseExpression(pa.Variable, table);
                TraverseExpression(pa.StartNode, table);
                TraverseExpression(pa.EndNode, table);
                break;
            case KarelFunctionCall f:
                table.AddReference(f.Identifier, f.Start);
                foreach (var arg in f.Args)
                {
                    TraverseExpression(arg, table);
                }
                break;
            case KarelComparisonExpression c:
                TraverseExpression(c.Lhs, table);
                TraverseExpression(c.Rhs, table);
                break;
            case KarelSumBinary s:
                TraverseExpression(s.Lhs, table);
                TraverseExpression(s.Rhs, table);
                break;
            case KarelProductBinary p:
                TraverseExpression(p.Lhs, table);
                TraverseExpression(p.Rhs, table);
                break;
            case KarelPositionBinary p:
                TraverseExpression(p.Lhs, table);
                TraverseExpression(p.Rhs, table);
                break;
            case KarelNotExpression n:
                TraverseExpression(n.Expr, table);
                break;
            case KarelUnaryMinus m:
                TraverseExpression(m.Expr, table);
                break;
            case KarelInteger _:
            case KarelReal _:
            case KarelString _:
            case KarelBool _:
                break;
        }
    }

    private static void TraverseCondition(KarelGlobalCondition cond, KarelSymbolTable table)
    {
        switch (cond)
        {
            case KarelErrorCondition c:
                TraverseExpression(c.Number, table);
                break;
            case KarelEventCondition c:
                TraverseExpression(c.Number, table);
                break;
            case KarelSemaphoreCondition c:
                TraverseExpression(c.Number, table);
                break;
            case KarelAbortCondition c:
                if (c.ProgramNumber != null)
                {
                    TraverseExpression(c.ProgramNumber, table);
                }
                break;
            case KarelPauseCondition c:
                if (c.ProgramNumber != null)
                {
                    TraverseExpression(c.ProgramNumber, table);
                }
                break;
            case KarelContinueCondition c:
                if (c.ProgramNumber != null)
                {
                    TraverseExpression(c.ProgramNumber, table);
                }
                break;
            case KarelPowerUpCondition _:
                break;
            case KarelComparisonCondition c:
                TraverseExpression(c.Variable, table);
                TraverseExpression(c.Expr, table);
                break;
            case KarelPortCondition c:
                TraverseExpression(c.Index, table);
                break;
            case KarelAndCondition c:
                foreach (var sub in c.Conditions)
                {
                    TraverseCondition(sub, table);
                }
                break;
            case KarelOrCondition c:
                foreach (var sub in c.Conditions)
                {
                    TraverseCondition(sub, table);
                }
                break;
        }
    }

    private static void TraverseWhen(KarelWhen when, KarelSymbolTable table)
    {
        TraverseWhenCondition(when.Condition, table);
        foreach (var action in when.Actions)
        {
            TraverseAction(action, table);
        }
    }

    private static void TraverseWhenCondition(KarelWhenCondition cond, KarelSymbolTable table)
    {
        switch (cond)
        {
            case KarelWhenOr o:
                foreach (var c in o.Conditions)
                {
                    TraverseCondition(c, table);
                }
                break;
            case KarelWhenAnd a:
                foreach (var c in a.Conditions)
                {
                    TraverseCondition(c, table);
                }
                break;
        }
    }

    private static void TraverseAction(KarelAction action, KarelSymbolTable table)
    {
        switch (action)
        {
            case KarelGroupAction a:
                TraverseStatement(a.Statement, table);
                break;
            case KarelConditionAction a:
                TraverseStatement(a.Statement, table);
                break;
            case KarelEventAction a:
                TraverseStatement(a.Signal, table);
                break;
            case KarelSemaphoreAction a:
                TraverseExpression(a.Number, table);
                break;
            case KarelPulseDoutAction a:
                TraverseExpression(a.Index, table);
                TraverseExpression(a.Time, table);
                break;
            case KarelPulseRdoAction a:
                TraverseExpression(a.Index, table);
                TraverseExpression(a.Time, table);
                break;
            case KarelAbortAction a:
                if (a.ProgramNumber != null)
                {
                    TraverseExpression(a.ProgramNumber, table);
                }
                break;
            case KarelPauseAction a:
                if (a.ProgramNumber != null)
                {
                    TraverseExpression(a.ProgramNumber, table);
                }
                break;
            case KarelContinueAction a:
                if (a.ProgramNumber != null)
                {
                    TraverseExpression(a.ProgramNumber, table);
                }
                break;
            case KarelVarAssignmentAction a:
                TraverseExpression(a.Variable, table);
                TraverseExpression(a.Expr, table);
                break;
            case KarelPortAssignmentAction a:
                TraverseExpression(a.Index, table);
                TraverseExpression(a.Expr, table);
                break;
            case KarelNoAbortAction _:
            case KarelNoPauseAction _:
            case KarelUnpauseAction _:
            case KarelNoMessageAction _:
            case KarelRestoreAction _:
                break;
        }
    }

    private static void TraverseCase(KarelCase c, KarelSymbolTable table)
    {
        switch (c)
        {
            case KarelValueCase vc:
                foreach (var s in vc.Body) TraverseStatement(s, table);
                break;
            case KarelElseCase ec:
                foreach (var s in ec.Body) TraverseStatement(s, table);
                break;
        }
    }

    private static void TraverseItem(KarelItem item, KarelSymbolTable table)
    {
        switch (item)
        {
            case KarelReadItemExpr i:
                TraverseExpression(i.Expression, table);
                foreach (var fmt in i.FormatSpecs) TraverseExpression(fmt, table);
                break;
            case KarelReadItemCR i:
                foreach (var fmt in i.FormatSpecs) TraverseExpression(fmt, table);
                break;
        }
    }
}
