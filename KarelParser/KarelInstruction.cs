using ParserUtils;
using Sprache;
using KarelParser.Instructions;

namespace KarelParser;

public abstract record KarelStatement() : WithPosition, IKarelParser<KarelStatement>
{
    public static Parser<KarelStatement> GetParser()
        => KarelAbort.GetParser()
            .Or(KarelAssignment.GetParser())
            .Or(KarelAttach.GetParser())
            .Or(KarelCall.GetParser())
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
            .Or(KarelWrite.GetParser());
}

/*
 * Expression
 */
public abstract record KarelExpression() : WithPosition, IKarelParser<KarelExpression>
{
    public static Parser<KarelExpression> GetParser()
        => KarelComparisonExpression.GetParser()
            .Or(KarelSumExpression.GetParser());
}

public sealed record KarelComparisonExpression(
        KarelSumExpression Lhs,
        KarelComparisonOperator Op,
        KarelSumExpression Rhs)
    : KarelExpression, IKarelParser<KarelExpression>
{
    public new static Parser<KarelExpression> GetParser()
        => from lhs in KarelSumExpression.GetParser()
           from op in KarelComparisonOperatorParser.Parser()
           from rhs in KarelSumExpression.GetParser()
           select new KarelComparisonExpression((KarelSumExpression)lhs, op, (KarelSumExpression)rhs);
}

/*
 * Sum
 */
public abstract record KarelSumExpression : KarelExpression, IKarelParser<KarelExpression>
{
    public new static Parser<KarelExpression> GetParser()
        => KarelSumBinary.GetParser()
            .Or(KarelProductExpression.GetParser());
}

public sealed record KarelSumBinary(
        KarelSumExpression Lhs,
        KarelSumOperator Op,
        KarelProductExpression Rhs)
    : KarelSumExpression, IKarelParser<KarelSumExpression>
{
    public new static Parser<KarelSumExpression> GetParser()
        => from lhs in Parse.Ref(() => KarelSumExpression.GetParser())
           from op in KarelSumOperatorParser.Parser()
           from rhs in KarelProductExpression.GetParser()
           select new KarelSumBinary((KarelSumExpression)lhs, op, (KarelProductExpression)rhs);
}

/*
 * Product
 */
public abstract record KarelProductExpression : KarelExpression, IKarelParser<KarelExpression>
{
    public new static Parser<KarelExpression> GetParser()
        => KarelProductBinary.GetParser()
            .Or(KarelFactorExpression.GetParser());
}

public sealed record KarelProductBinary(
        KarelProductExpression Lhs,
        KarelProductOperator Op,
        KarelFactorExpression Rhs)
    : KarelProductExpression, IKarelParser<KarelProductExpression>
{
    public new static Parser<KarelProductExpression> GetParser()
        => from lhs in Parse.Ref(() => KarelProductExpression.GetParser())
           from op in KarelProductOperatorParser.Parser()
           from rhs in KarelFactorExpression.GetParser()
           select new KarelProductBinary((KarelProductExpression)lhs, op, (KarelFactorExpression)rhs);
}

/*
 * Factor
 */
public abstract record KarelFactorExpression : KarelExpression, IKarelParser<KarelExpression>
{
    public new static Parser<KarelExpression> GetParser()
        => KarelNotExpression.GetParser()
            .Or(KarelPositionBinary.GetParser())
            .Or(KarelPrimaryExpression.GetParser());
}

public sealed record KarelNotExpression(KarelPrimaryExpression Expr)
    : KarelFactorExpression, IKarelParser<KarelFactorExpression>
{
    public new static Parser<KarelFactorExpression> GetParser()
        => from kw in KarelCommon.Keyword("NOT")
           from expr in KarelPrimaryExpression.GetParser()
           select new KarelNotExpression((KarelPrimaryExpression)expr);
}

public sealed record KarelPositionBinary(
        KarelFactorExpression Lhs,
        KarelPositionOperator Operator,
        KarelPrimaryExpression Rhs)
    : KarelFactorExpression, IKarelParser<KarelFactorExpression>
{
    public new static Parser<KarelFactorExpression> GetParser()
        => from lhs in Parse.Ref(() => KarelFactorExpression.GetParser())
           from op in KarelPositionOperatorParser.Parser()
           from rhs in KarelPrimaryExpression.GetParser()
           select new KarelPositionBinary((KarelFactorExpression)lhs, op, (KarelPrimaryExpression)rhs);
}

/*
 * Primary
 */
public abstract record KarelPrimaryExpression : KarelExpression, IKarelParser<KarelExpression>
{
    public new static Parser<KarelExpression> GetParser()
        => KarelFunctionCall.GetParser()
            .Or(KarelValue.GetParser())
            .Or(Parse.Ref(() => KarelExpression.GetParser())
                    .BetweenParen()).WithPos();
}

public sealed record KarelFunctionCall(string Identifier, List<KarelExpression> Args)
    : KarelPrimaryExpression, IKarelParser<KarelPrimaryExpression>
{
    public new static Parser<KarelPrimaryExpression> GetParser()
        => from ident in KarelCommon.Identifier.WithPosition()
           from args in Parse.Ref(() => KarelExpression.GetParser())
                        .DelimitedBy(KarelCommon.Keyword(","), 1, null)
           select new KarelFunctionCall(ident.Value, args.ToList())
           {
               Start = ident.Start,
               End = ident.End
           };
}

public abstract record KarelValue : KarelPrimaryExpression, IKarelParser<KarelPrimaryExpression>
{
    public new static Parser<KarelPrimaryExpression> GetParser()
        => KarelString.GetParser()
            .Or(KarelInteger.GetParser())
            .Or(KarelVariableAcess.GetParser());
}

public sealed record KarelString(string Value) : KarelValue, IKarelParser<KarelValue>
{
    public new static Parser<KarelValue> GetParser()
        => Parse.AnyChar.Many()
            .Contained(Parse.Char('\''), Parse.Char('\''))
            .Text()
            .Token()
            .WithPosition()
            .Select(res => new KarelString(res.Value) { Start = res.Start, End = res.End });
}

public sealed record KarelInteger(int Value) : KarelValue, IKarelParser<KarelValue>
{
    public new static Parser<KarelValue> GetParser()
        => from negated in ParserUtils.ParserExtensions.Keyword("-").Optional()
           from num in Parse.Number.Select(int.Parse)
           select new KarelInteger(negated switch
           {
               { IsDefined: true } => -num,
               _ => num
           });
}

public sealed record KarelSystemIndentifier : KarelValue, IKarelParser<KarelValue>
{
    public new static Parser<KarelValue> GetParser()
        => throw new NotImplementedException();
}
