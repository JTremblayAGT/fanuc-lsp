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
            select new KarelNotExpression((KarelPrimaryExpression)expr);

    private static readonly Parser<KarelExpression> FactorExpr =
        Parse.ChainOperator(KarelPositionOperatorParser.Parser().Token().Once(),
            Not.Or(Primary),
            (op, left, right) => new KarelPositionBinary(
                (KarelFactorExpression)left, op.First(), (KarelPrimaryExpression)right));

    private static readonly Parser<KarelExpression> ProductExpr =
        Parse.ChainOperator(KarelProductOperatorParser.Parser().Token().Once(),
            FactorExpr,
            (op, left, right) => new KarelProductBinary(
                (KarelProductExpression)left, op.First(), (KarelFactorExpression)right));

    private static readonly Parser<KarelExpression> SumExpr =
        Parse.ChainOperator(KarelSumOperatorParser.Parser().Token().Once(),
            ProductExpr,
            (op, left, right) => new KarelSumBinary(
                (KarelSumExpression)left, op.First(), (KarelProductExpression)right));

    private static readonly Parser<KarelExpression> ComparisonExpr =
        Parse.ChainOperator(KarelComparisonOperatorParser.Parser().Token().Once(),
            SumExpr,
            (op, left, right) => new KarelComparisonExpression(
                (KarelSumExpression)left, op.First(), (KarelSumExpression)right));

    private static readonly Parser<KarelExpression> Expression 
        = ComparisonExpr
            .Or(SumExpr)
            .Or(ProductExpr)
            .Or(FactorExpr)
            .Or(Primary);

    public static Parser<KarelExpression> GetParser() => Expression;
}

public sealed record KarelComparisonExpression(
    KarelSumExpression Lhs,
    KarelComparisonOperator Op,
    KarelSumExpression Rhs)
    : KarelExpression;

public abstract record KarelSumExpression : KarelExpression;

public sealed record KarelSumBinary(
    KarelSumExpression Lhs,
    KarelSumOperator Op,
    KarelProductExpression Rhs)
    : KarelSumExpression;

public abstract record KarelProductExpression : KarelExpression;

public sealed record KarelProductBinary(
    KarelProductExpression Lhs,
    KarelProductOperator Op,
    KarelFactorExpression Rhs)
    : KarelProductExpression;

public abstract record KarelFactorExpression : KarelExpression;

public sealed record KarelNotExpression(KarelPrimaryExpression Expr)
    : KarelFactorExpression;

public sealed record KarelPositionBinary(
    KarelFactorExpression Lhs,
    KarelPositionOperator Operator,
    KarelPrimaryExpression Rhs)
    : KarelFactorExpression;

public abstract record KarelPrimaryExpression : KarelExpression;

public sealed record KarelFunctionCall(string Identifier, List<KarelExpression> Args)
    : KarelPrimaryExpression, IKarelParser<KarelPrimaryExpression>
{
    public new static Parser<KarelPrimaryExpression> GetParser()
        => from ident in KarelCommon.Identifier.WithPosition()
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
    public new static Parser<KarelValue> GetParser()
        => KarelString.GetParser()
            .Or(KarelInteger.GetParser())
            .Or(KarelBool.GetParser())
            .Or(KarelVariableAccess.GetParser());
}

public sealed record KarelString(string Value) : KarelValue, IKarelParser<KarelValue>
{
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
    public new static Parser<KarelValue> GetParser()
        => from negated in KarelCommon.Keyword("-").Optional()
           from num in Parse.Number.Select(int.Parse)
           select new KarelInteger(negated switch
           {
               { IsDefined: true } => -num,
               _ => num
           });
}

public sealed record KarelBool(bool Value) : KarelValue, IKarelParser<KarelValue>
{
    public new static Parser<KarelValue> GetParser()
        => KarelCommon.Keyword("TRUE").Return(new KarelBool(true))
            .Or(KarelCommon.Keyword("FALSE").Return(new KarelBool(false)));
}
