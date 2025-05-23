using Sprache;

namespace TPLangParser.TPLang.Instructions;

public record TpPosRegInstruction() : TpInstruction(0), ITpParser<TpPosRegInstruction>
{
    public new static Parser<TpPosRegInstruction> GetParser()
        => TpPosRegAssignmentInstruction.GetParser()
            .Or(TpPosRegElementAssignmentInstruction.GetParser())
            .Or(TpLockPregInstruction.GetParser())
            .Or(TpUnlockPregInstruction.GetParser());
}

// Recursive grammar similar to mixed logic expressions
public record TpPosRegExpression
    : ITpParser<TpPosRegExpression>
{
    private static readonly Parser<TpPosRegExpression> ExpressionRef = Parse.Ref(() => Expression);

    private static readonly Parser<TpPosRegExpression> Value = 
        TpValue.Position.Select(value => new TpPosRegValue(value));

    private static readonly Parser<TpPosRegBinary> Addition =
        from value in Value
        from op in TpCommon.Keyword("+")
        from expr in ExpressionRef
        select new TpPosRegAddition((TpPosRegValue)value, expr);

    private static readonly Parser<TpPosRegBinary> Subtraction =
        from value in Value
        from op in TpCommon.Keyword("-")
        from expr in ExpressionRef
        select new TpPosRegSubtraction((TpPosRegValue)value, expr);


    private static readonly Parser<TpPosRegBinary> Binary =
        Addition
        .Or(Subtraction);

    private static readonly Parser<TpPosRegExpression> Expression =
        Binary
        .Or(Value);

    public static Parser<TpPosRegExpression> GetParser()
        => Expression;
}

public sealed record TpPosRegValue(TpValue Value) : TpPosRegExpression;

public record TpPosRegBinary(TpPosRegValue Lhs, TpPosRegExpression Rhs) : TpPosRegExpression;

public sealed record TpPosRegAddition(TpPosRegValue Lhs, TpPosRegExpression Rhs) : TpPosRegBinary(Lhs, Rhs);

public sealed record TpPosRegSubtraction(TpPosRegValue Lhs, TpPosRegExpression Rhs) : TpPosRegBinary(Lhs, Rhs);

public sealed record TpPosRegAssignmentInstruction(TpPositionRegister PosReg, TpPosRegExpression Expr)
    : TpPosRegInstruction, ITpParser<TpPosRegInstruction>
{
    public new static Parser<TpPosRegInstruction> GetParser()
        => from posReg in TpPositionRegister.GetParser()
            from sep in TpCommon.Keyword("=")
            from expr in TpPosRegExpression.GetParser()
            select new TpPosRegAssignmentInstruction(posReg, expr);
}


public sealed record TpPosRegElementAssignmentInstruction(TpPositionRegister PosReg, TpArithmeticExpression Expr)
    : TpPosRegInstruction, ITpParser<TpPosRegInstruction>
{
    public new static Parser<TpPosRegInstruction> GetParser()
        => from posReg in TpPositionRegister.Element
            from sep in TpCommon.Keyword("=")
            from expr in TpArithmeticExpression.GetParser()
            select new TpPosRegElementAssignmentInstruction(posReg, expr);
}

public sealed record TpLockPregInstruction : TpPosRegInstruction, ITpParser<TpPosRegInstruction>
{
    public new static Parser<TpPosRegInstruction> GetParser()
        => TpCommon.Keyword("LOCK PREG").Return(new TpLockPregInstruction());
}

public sealed record TpUnlockPregInstruction : TpPosRegInstruction, ITpParser<TpPosRegInstruction>
{
    public new static Parser<TpPosRegInstruction> GetParser()
        => TpCommon.Keyword("UNLOCK PREG").Return(new TpUnlockPregInstruction());
}
