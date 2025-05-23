using Sprache;

namespace TPLangParser.TPLang;
// Recursive grammar similar to mixed logic expressions
public record TpArithmeticExpression
    : ITpParser<TpArithmeticExpression>
{
    private static readonly Parser<TpArithmeticExpression> Value
        = TpValue.GetParser().Select(value => new TpArithmeticValue(value));

    private static readonly Parser<TpArithmeticBinary> Addition
        = from value in Value
        from op in TpArithmeticOperatorParser.Plus
        from expr in Value.Or(Parse.Ref(() => BinaryAdd))
        select new TpArithmeticAddition((TpArithmeticValue)value, expr);

    private static readonly Parser<TpArithmeticBinary> Subtraction
        = from value in Value
        from op in TpArithmeticOperatorParser.Minus
        from expr in Value.Or(Parse.Ref(() => BinaryAdd))
        select new TpArithmeticSubtraction((TpArithmeticValue)value, expr);

    private static readonly Parser<TpArithmeticBinary> Mult
        = from value in Value
        from op in TpArithmeticOperatorParser.Times
        from expr in Value.Or(Parse.Ref(() => BinaryMult))
        select new TpArithmeticMult((TpArithmeticValue)value, expr);

    private static readonly Parser<TpArithmeticBinary> Div
        = from value in Value
        from op in TpArithmeticOperatorParser.Div
        from expr in Value.Or(Parse.Ref(() => BinaryMult))
        select new TpArithmeticDiv((TpArithmeticValue)value, expr);

    private static readonly Parser<TpArithmeticBinary> IntDiv
        = from value in Value
        from op in TpArithmeticOperatorParser.IntDiv
        from expr in Value.Or(Parse.Ref(() => BinaryInt))
        select new TpArithmeticIntDiv((TpArithmeticValue)value, expr);

    private static readonly Parser<TpArithmeticBinary> Mod
        = from value in Value
        from op in TpArithmeticOperatorParser.Mod
        from expr in Value.Or(Parse.Ref(() => BinaryInt))
        select new TpArithmeticMod((TpArithmeticValue)value, expr);


    private static readonly Parser<TpArithmeticBinary> BinaryAdd
        = Addition
            .Or(Subtraction);

    private static readonly Parser<TpArithmeticBinary> BinaryMult
        = Mult
            .Or(Div);

    private static readonly Parser<TpArithmeticBinary> BinaryInt
        = IntDiv
            .Or(Mod);

    private static readonly Parser<TpArithmeticExpression> Expression
        = BinaryInt
            .Or(BinaryAdd)
            .Or(BinaryMult)
            .Or(Value);

    public static Parser<TpArithmeticExpression> GetParser()
        => Expression;
}

public sealed record TpArithmeticValue(TpValue Value) : TpArithmeticExpression;

public record TpArithmeticBinary(TpArithmeticValue Lhs, TpArithmeticExpression Rhs)
    : TpArithmeticExpression;

public sealed record TpArithmeticAddition(TpArithmeticValue Lhs, TpArithmeticExpression Rhs)
    : TpArithmeticBinary(Lhs, Rhs);

public sealed record TpArithmeticSubtraction(TpArithmeticValue Lhs, TpArithmeticExpression Rhs)
    : TpArithmeticBinary(Lhs, Rhs);

public sealed record TpArithmeticMult(TpArithmeticValue Lhs, TpArithmeticExpression Rhs)
    : TpArithmeticBinary(Lhs, Rhs);

public sealed record TpArithmeticDiv(TpArithmeticValue Lhs, TpArithmeticExpression Rhs)
    : TpArithmeticBinary(Lhs, Rhs);

public sealed record TpArithmeticMod(TpArithmeticValue Lhs, TpArithmeticExpression Rhs)
    : TpArithmeticBinary(Lhs, Rhs);

public sealed record TpArithmeticIntDiv(TpArithmeticValue Lhs, TpArithmeticExpression Rhs)
    : TpArithmeticBinary(Lhs, Rhs);
