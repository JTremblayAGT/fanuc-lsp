using Sprache;

namespace TPLangParser.TPLang;

// Mixed logic's grammar is recursive and the regular parser pattern in this project causes a stack overflow
internal readonly struct TpMixedLogicExpressionParser
{
    // Create a forward reference to break the circular dependency
    private static readonly Parser<TpMixedLogicExpression> ExpressionRef 
        = Parse.Ref(() => Expression);

    private static readonly Parser<TpMixedLogicExpression> Value
        = TpValue.GetParser().Select(val => new TpMixedLogicValue(val));

    private static readonly Parser<TpMixedLogicExpression> UnaryNot
        = from keyword in Parse.Char('!').Token()
          from term in Term
          select new TpMixedLogicUnaryNot(term);

    private static readonly Parser<TpMixedLogicExpression> Term
        = Value
            .Or(UnaryNot)
            .Or(from lparen in Parse.Char('(').Token()
                from expr in ExpressionRef // Use the forward reference here
                from rparen in Parse.Char(')').Token()
                select expr);

    private static readonly Parser<TpMixedLogicBinary> BinaryLogical
        = from lhs in Term // Use Term instead of Expression for left side
          from op in TpLogicalOperatorParser.Parser.Token()
          from rhs in ExpressionRef // Use the forward reference for right side
          select new TpMixedLogicBinaryLogical(op, lhs, rhs);

    private static readonly Parser<TpMixedLogicBinary> BinaryComparison
        = from lhs in Term // Use Term instead of Expression for left side
          from op in TpComparisonOperatorParser.Parser.Token()
          from rhs in ExpressionRef // Use the forward reference for right side
          select new TpMixedLogicBinaryComparison(op, lhs, rhs);

    private static readonly Parser<TpMixedLogicBinary> BinaryArithmetic
        = from lhs in Term // Use Term instead of Expression for left side
          from op in TpArithmeticOperatorParser.Parser.Token()
          from rhs in ExpressionRef // Use the forward reference for right side
          select new TpMixedLogicBinaryArithmetic(op, lhs, rhs);

    private static readonly Parser<TpMixedLogicExpression> Binary
        = BinaryLogical
            .Or(BinaryComparison)
            .Or(BinaryArithmetic);

    public static readonly Parser<TpMixedLogicExpression> Expression = Binary.Or(Term);
}

public record TpMixedLogicExpression : ITpParser<TpMixedLogicExpression>
{
    public static Parser<TpMixedLogicExpression> GetParser() 
        => TpMixedLogicExpressionParser.Expression;
}

public record TpMixedLogicValue(TpValue Value) : TpMixedLogicExpression;

public record TpMixedLogicUnaryNot(TpMixedLogicExpression Term) : TpMixedLogicExpression;

public record TpMixedLogicBinary(TpMixedLogicExpression Lhs, TpMixedLogicExpression Rhs)
    : TpMixedLogicExpression;

public record TpMixedLogicBinaryLogical(TpLogicalOperator Operator, TpMixedLogicExpression Lhs, TpMixedLogicExpression Rhs) 
    : TpMixedLogicBinary(Lhs, Rhs);

public record TpMixedLogicBinaryComparison(TpComparisonOperator Operator, TpMixedLogicExpression Lhs, TpMixedLogicExpression Rhs) 
    : TpMixedLogicBinary(Lhs, Rhs);

public record TpMixedLogicBinaryArithmetic(TpArithmeticOperator Operator, TpMixedLogicExpression Lhs, TpMixedLogicExpression Rhs) 
    : TpMixedLogicBinary(Lhs, Rhs);

