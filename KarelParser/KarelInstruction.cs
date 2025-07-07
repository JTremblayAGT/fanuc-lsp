using ParserUtils;
using Sprache;
using KarelParser.Instructions;

namespace KarelParser;

public abstract record KarelStatement : WithPosition, IKarelParser<KarelStatement>
{
    public static Parser<KarelStatement> GetParser()
        => KarelAssignment.GetParser()
            .Or(KarelLabel.GetParser())
            .Or(KarelCall.GetParser())
            .Or(KarelAttach.GetParser())
            .Or(KarelAbort.GetParser())
            .Or(KarelCancel.GetParser())
            .Or(KarelCancelFile.GetParser())
            .Or(KarelCloseFile.GetParser())
            .Or(KarelCloseHand.GetParser())
            .Or(KarelCondition.GetParser())
            .Or(KarelConnectTimer.GetParser())
            .Or(KarelDelay.GetParser())
            .Or(KarelDisable.GetParser())
            .Or(KarelDisconnectTimer.GetParser())
            .Or(KarelEnable.GetParser())
            .Or(KarelFor.GetParser())
            .Or(KarelGoto.GetParser())
            .Or(KarelHold.GetParser())
            .Or(KarelIfThenElse.GetParser())
            .Or(KarelIfThen.GetParser())
            .Or(KarelOpenFile.GetParser())
            .Or(KarelOpenHand.GetParser())
            .Or(KarelPause.GetParser())
            .Or(KarelPulse.GetParser())
            .Or(KarelPurge.GetParser())
            .Or(KarelRead.GetParser())
            .Or(KarelRelaxHand.GetParser())
            .Or(KarelRelease.GetParser())
            .Or(KarelRepeat.GetParser())
            .Or(KarelResume.GetParser())
            .Or(KarelReturn.GetParser())
            .Or(KarelSelect.GetParser())
            .Or(KarelSignal.GetParser())
            .Or(KarelStop.GetParser())
            .Or(KarelUnhold.GetParser())
            .Or(KarelUsing.GetParser())
            .Or(KarelWait.GetParser())
            .Or(KarelWhile.GetParser())
            .Or(KarelWrite.GetParser())
            .IgnoreComments()
            .WithPos();

}

public abstract record KarelExpression : WithPosition, IKarelParser<KarelExpression>
{
    public static readonly Parser<KarelExpression> ExprRef = Parse.Ref(() => Expression);

    private static readonly Parser<KarelExpression> Primary
        = KarelFunctionCall.GetParser()
            .Or(KarelValue.GetParser())
            .Or(ExprRef.BetweenParen());

    private static readonly Parser<KarelFactorExpression> Not
        = from kw in KarelCommon.Keyword("NOT")
          from expr in Primary
          select new KarelNotExpression(expr);

    private static readonly Parser<KarelExpression> FactorExpr =
        Parse.ChainOperator(KarelPositionOperatorParser.Parser(),
            Not.Or(Primary),
            (op, left, right) => new KarelPositionBinary(
                left, op, right));

    private static readonly Parser<KarelExpression> ProductExpr =
        Parse.ChainOperator(KarelProductOperatorParser.Parser(),
            FactorExpr,
            (op, left, right) => new KarelProductBinary(
                left, op, right));

    private static readonly Parser<KarelExpression> SumExpr =
        Parse.ChainOperator(KarelSumOperatorParser.Parser(),
            ProductExpr,
            (op, left, right) => new KarelSumBinary(
                left, op, right));

    private static readonly Parser<KarelExpression> ComparisonExpr =
        Parse.ChainOperator(KarelComparisonOperatorParser.Parser(),
            SumExpr,
            (op, left, right) => new KarelComparisonExpression(
                left, op, right));

    private static readonly Parser<KarelExpression> Expression
        = ComparisonExpr
            .Or(SumExpr)
            .Or(ProductExpr)
            .Or(FactorExpr)
            .Or(Primary);

    public static Parser<KarelExpression> GetParser() => Expression;
}

public sealed record KarelComparisonExpression(
    KarelExpression Lhs,
    KarelComparisonOperator Op,
    KarelExpression Rhs)
    : KarelExpression;

public sealed record KarelSumBinary(
    KarelExpression Lhs,
    KarelSumOperator Op,
    KarelExpression Rhs)
    : KarelExpression;

public abstract record KarelProductExpression : KarelExpression;

public sealed record KarelProductBinary(
    KarelExpression Lhs,
    KarelProductOperator Op,
    KarelExpression Rhs)
    : KarelProductExpression;

public abstract record KarelFactorExpression : KarelExpression;

public sealed record KarelNotExpression(KarelExpression Expr)
    : KarelFactorExpression;

public sealed record KarelPositionBinary(
    KarelExpression Lhs,
    KarelPositionOperator Operator,
    KarelExpression Rhs)
    : KarelFactorExpression;

public abstract record KarelPrimaryExpression : KarelExpression;

public sealed record KarelFunctionCall(string Identifier, List<KarelExpression> Args)
    : KarelPrimaryExpression, IKarelParser<KarelPrimaryExpression>
{
    public new static Parser<KarelPrimaryExpression> GetParser()
        => from ident in KarelCommon.Identifier.Or(KarelCommon.Intrinsic).WithPosition()
           from args in ExprRef
                        .DelimitedBy(KarelCommon.Keyword(","), 1, null)
                        .BetweenParen()
           select new KarelFunctionCall(ident.Value, args.ToList())
           {
               Start = ident.Start,
               End = ident.End
           };
}

public abstract record KarelValue : KarelPrimaryExpression, IKarelParser<KarelValue>
{
    public abstract override string ToString();

    public new static Parser<KarelValue> GetParser()
        => KarelString.GetParser()
            .Or(KarelReal.GetParser())
            .Or(KarelInteger.GetParser())
            .Or(KarelBool.GetParser())
            .Or(KarelVariableAccess.GetParser());
}

public sealed record KarelString(string Value) : KarelValue, IKarelParser<KarelValue>
{
    public override string ToString()
        => $"\"{Value}\"";

    public new static Parser<KarelValue> GetParser()
        => Parse.Char('\'').Then(_ =>
            Parse.AnyChar.Until(Parse.Char('\''))
            .Text()
            .Token()
            .WithPosition()
            .Select(res =>
                new KarelString(res.Value) { Start = res.Start, End = res.End }));
}

public sealed record KarelInteger(int Value) : KarelValue, IKarelParser<KarelValue>
{
    public override string ToString()
        => Value.ToString();

    public new static Parser<KarelValue> GetParser()
        => from negated in KarelCommon.Keyword("-").Optional()
           from num in Parse.Number.Select(int.Parse)
           select new KarelInteger(negated switch
           {
               { IsDefined: true } => -num,
               _ => num
           });
}

public sealed record KarelReal(float Value) : KarelValue, IKarelParser<KarelValue>
{
    public override string ToString()
        => Value.ToString();

    public new static Parser<KarelValue> GetParser()
        => from negated in KarelCommon.Keyword("-").Optional()
           from num in Parse.Decimal.Select(float.Parse)
           select new KarelReal(negated switch
           {
               { IsDefined: true } => -num,
               _ => num
           });
}

public sealed record KarelBool(bool Value) : KarelValue, IKarelParser<KarelValue>
{
    public override string ToString()
        => Value.ToString();

    public new static Parser<KarelValue> GetParser()
        => KarelCommon.Keyword("TRUE").Return(new KarelBool(true))
            .Or(KarelCommon.Keyword("FALSE").Return(new KarelBool(false)));
}
