using Sprache;

namespace AGT.TPLangParser.TPLang.Instructions;

public abstract record TpBranchingInstruction() : TpInstruction(0), ITpParser<TpInstruction>
{
    public new static Parser<TpInstruction> GetParser()
        => TpLabelDefinitionInstruction.GetParser()
            .Or(TpJumpLabelInstruction.GetParser())
            .Or(TpCallInstruction.GetParser())
            .Or(TpIfInstruction.GetParser())
            .Or(TpSelectInstruction.GetParser())
            .Or(TpSelectCaseInstruction.GetParser())
            .Or(TpIfThenInstruction.GetParser())
            .Or(TpElseInstruction.GetParser())
            .Or(TpEndIfInstruction.GetParser())
            .Or(TpEndInstruction.GetParser());
}

public abstract record TpBranchingAction 
    : TpBranchingInstruction, ITpParser<TpBranchingAction>
{
    public new static Parser<TpBranchingAction> GetParser()
        => TpJumpLabelInstruction.GetParser()
            .Or(TpCallInstruction.GetParser())
            .Or(TpMixedLogicAssignmentBranchingAction.GetParser())
            .Or(TpPulseBranchingAction.GetParser());
}

public sealed record TpLabelDefinitionInstruction(TpLabel Label)
    : TpBranchingInstruction, ITpParser<TpBranchingInstruction>
{
    public new static Parser<TpBranchingInstruction> GetParser()
        => TpLabel.GetParser().Select(label => new TpLabelDefinitionInstruction(label));
}

public sealed record TpEndInstruction : TpBranchingInstruction, ITpParser<TpBranchingInstruction>
{
    public new static Parser<TpBranchingInstruction> GetParser() 
        => from keyword in Parse.String("END").Token()
            select new TpEndInstruction();
}

public sealed record TpJumpLabelInstruction(TpLabel Label)
    : TpBranchingAction, ITpParser<TpBranchingAction>
{
    public new static Parser<TpBranchingAction> GetParser()
        => from keyword in Parse.String("JMP").Token()
            from label in TpLabel.GetParser()
            select new TpJumpLabelInstruction(label);
}

public abstract record TpCallMethod : ITpParser<TpCallMethod>
{
    public static Parser<TpCallMethod> GetParser() 
        => TpCallByStringRegister.GetParser()
            .Or(TpCallByName.GetParser());
}

public sealed record TpCallByStringRegister(TpStringRegister StringRegister)
    : TpCallMethod, ITpParser<TpCallMethod>
{
    public new static Parser<TpCallMethod> GetParser() 
        => TpStringRegister.GetParser().Select(sr => new TpCallByStringRegister(sr));
}
public sealed record TpCallByName(string ProgramName)
    : TpCallMethod, ITpParser<TpCallMethod>
{
    public new static Parser<TpCallMethod> GetParser() 
        => TpCommon.ProgramName.Select(name => new TpCallByName(name));
}

public sealed record TpCallInstruction(TpCallMethod CallMethod, List<TpValue> Arguments)
    : TpBranchingAction, ITpParser<TpBranchingAction>
{
    private static readonly Parser<IEnumerable<TpValue>> Args =
        TpValue.GetParser().DelimitedBy(TpCommon.Keyword(","), 1, 10).BetweenParen();

    public new static Parser<TpBranchingAction> GetParser()
        => from keyword in TpCommon.Keyword("CALL")
            from programName in TpCallMethod.GetParser()
            from args in Args.Optional()
            select new TpCallInstruction(programName, args.GetOrElse([]).ToList());
}

public sealed record TpMixedLogicAssignmentBranchingAction(TpMixedLogicInstruction Instruction)
    : TpBranchingAction, ITpParser<TpBranchingAction>
{
    public new static Parser<TpBranchingAction> GetParser()
        => from mixedLogicAssignment in TpMixedLogicAssignment.GetParser()
            select new TpMixedLogicAssignmentBranchingAction(mixedLogicAssignment);
}

public sealed record TpPulseBranchingAction(TpValueOnOffIOPort IoPort, TpValuePulse Pulse)
    : TpBranchingAction, ITpParser<TpBranchingAction>
{
    public new static Parser<TpBranchingAction> GetParser()
        => from ioPort in TpValueOnOffIOPort.GetParser()
            from sep in TpCommon.Keyword("=")
            from pulse in TpValuePulse.GetParser()
            select new TpPulseBranchingAction(ioPort, (TpValuePulse)pulse);
}

public abstract record TpIfExpression : ITpParser<TpIfExpression>
{
    public static Parser<TpIfExpression> GetParser()
        => TpIfExpressionLogic.GetParser()
            .Or(TpIfExpressionMixedLogic.GetParser());
}

public sealed record TpIfExpressionLogic(TpLogicExpression Expression) : TpIfExpression, ITpParser<TpIfExpression>
{
    public new static Parser<TpIfExpression> GetParser()
        => TpLogicExpression.GetParser().Select(expr => new TpIfExpressionLogic(expr));
}


public sealed record TpIfExpressionMixedLogic(TpMixedLogicExpression Expression)
    : TpIfExpression, ITpParser<TpIfExpression>
{
    public new static Parser<TpIfExpression> GetParser()
        => TpMixedLogicExpression.GetParser().BetweenParen()
            .Select(expr => new TpIfExpressionMixedLogic(expr));
}

public sealed record TpIfInstruction(
    TpIfExpression Expression,
    TpBranchingAction Action) : TpBranchingInstruction, ITpParser<TpBranchingInstruction>
{
    public new static Parser<TpBranchingInstruction> GetParser()
        => from keyword in Parse.String("IF").Token()
            from expression in TpIfExpression.GetParser()
            from comma in Parse.Char(',').Token()
            from action in TpBranchingAction.GetParser()
            select new TpIfInstruction(expression, action);
}

public record TpSelectCase(TpValue Value, TpBranchingAction Action) : ITpParser<TpSelectCase>
{
    private static readonly Parser<TpValue> AllowedValues =
        TpValueIntegerConstant.GetParser()
            .Or(TpValueFloatingPointConstant.GetParser())
            .Or(TpValueRegister.GetParser());

    private static readonly Parser<TpSelectCase> InternalParser =
        from kw in TpCommon.Keyword("=")
        from value in AllowedValues
        from sep in TpCommon.Keyword(",")
        from action in TpBranchingAction.GetParser()
        select new TpSelectCase(value, action);

    public static Parser<TpSelectCase> GetParser() 
        => InternalParser.Or(TpSelectElseCase.GetParser());
}

public sealed record TpSelectElseCase(TpBranchingAction Action) : TpSelectCase(null!, Action), ITpParser<TpSelectCase>
{
    public new static Parser<TpSelectCase> GetParser()
        => from keyword in TpCommon.Keyword("ELSE")
            from sep in TpCommon.Keyword(",")
            from action in TpBranchingAction.GetParser()
            select new TpSelectElseCase(action);
}
public sealed record TpSelectCaseInstruction(TpSelectCase SelectCase)
    : TpBranchingInstruction, ITpParser<TpBranchingInstruction>
{
    public new static Parser<TpBranchingInstruction> GetParser() 
        => TpSelectCase.GetParser().Select(selCase => new TpSelectCaseInstruction(selCase));
}

public sealed record TpSelectInstruction(
    TpRegister Register,
    TpSelectCase Case) : TpBranchingInstruction, ITpParser<TpBranchingInstruction>
{
    public new static Parser<TpBranchingInstruction> GetParser()
        => from keyword in Parse.String("SELECT").Token()
            from register in TpRegister.GetParser()
            from firstCase in TpSelectCase.GetParser()
            select new TpSelectInstruction(register, firstCase);
}

public sealed record TpIfThenInstruction(TpMixedLogicExpression Expression) 
    : TpBranchingInstruction, ITpParser<TpBranchingInstruction>
{
    public new static Parser<TpBranchingInstruction> GetParser() 
        => from keyword in Parse.String("IF").Token()
            from expression in TpMixedLogicExpression.GetParser()
            from tail in Parse.String("THEN").Token()
            select new TpIfThenInstruction(expression);
}

public sealed record TpElseInstruction 
    : TpBranchingInstruction, ITpParser<TpBranchingInstruction>
{
    public new static Parser<TpBranchingInstruction> GetParser() 
        => Parse.String("ELSE").Return(new TpElseInstruction()).Token();
}

public sealed record TpEndIfInstruction 
    : TpBranchingInstruction, ITpParser<TpBranchingInstruction>
{
    public new static Parser<TpBranchingInstruction> GetParser() 
        => Parse.String("ENDIF").Return(new TpEndIfInstruction()).Token();
}
