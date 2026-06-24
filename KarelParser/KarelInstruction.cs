using System.Globalization;

using ParserUtils;
using Sprache;
using KarelParser.Instructions;

namespace KarelParser;

public abstract record KarelStatement : WithPosition, IKarelParser<KarelStatement>
{
    public static Parser<KarelStatement> GetParser()
        => KarelAssignment.GetParser().WithErrorContext("Assignment")
            .Or(KarelLabel.GetParser().WithErrorContext("Label"))
            .Or(KarelCall.GetParser().WithErrorContext("Call"))
            .Or(KarelAttach.GetParser().WithErrorContext("Attach"))
            .Or(KarelAbort.GetParser().WithErrorContext("Abort"))
            .Or(KarelCancel.GetParser().WithErrorContext("Cancel"))
            .Or(KarelCancelFile.GetParser().WithErrorContext("CancelFile"))
            .Or(KarelCloseFile.GetParser().WithErrorContext("CloseFile"))
            .Or(KarelCloseHand.GetParser().WithErrorContext("CloseHand"))
            .Or(KarelCondition.GetParser().WithErrorContext("Condition"))
            .Or(KarelConnectTimer.GetParser().WithErrorContext("ConnectTimer"))
            .Or(KarelDelay.GetParser().WithErrorContext("Delay"))
            .Or(KarelDisable.GetParser().WithErrorContext("Disable"))
            .Or(KarelDisconnectTimer.GetParser().WithErrorContext("DisconnectTimer"))
            .Or(KarelEnable.GetParser().WithErrorContext("Enable"))
            .Or(KarelFor.GetParser().WithErrorContext("For"))
            .Or(KarelGoto.GetParser().WithErrorContext("Goto"))
            .Or(KarelHold.GetParser().WithErrorContext("Hold"))
            .Or(KarelIfThenElse.GetParser().WithErrorContext("IfThenElse"))
            .Or(KarelIfThen.GetParser().WithErrorContext("IfThen"))
            .Or(KarelOpenFile.GetParser().WithErrorContext("OpenFile"))
            .Or(KarelOpenHand.GetParser().WithErrorContext("OpenHand"))
            .Or(KarelPause.GetParser().WithErrorContext("Pause"))
            .Or(KarelPulse.GetParser().WithErrorContext("Pulse"))
            .Or(KarelPurge.GetParser().WithErrorContext("Purge"))
            .Or(KarelRead.GetParser().WithErrorContext("Read"))
            .Or(KarelRelaxHand.GetParser().WithErrorContext("RelaxHand"))
            .Or(KarelRelease.GetParser().WithErrorContext("Release"))
            .Or(KarelRepeat.GetParser().WithErrorContext("Repeat"))
            .Or(KarelResume.GetParser().WithErrorContext("Resume"))
            .Or(KarelReturn.GetParser().WithErrorContext("Return"))
            .Or(KarelSelect.GetParser().WithErrorContext("Select"))
            .Or(KarelSignal.GetParser().WithErrorContext("Signal"))
            .Or(KarelStop.GetParser().WithErrorContext("Stop"))
            .Or(KarelUnhold.GetParser().WithErrorContext("Unhold"))
            .Or(KarelUsing.GetParser().WithErrorContext("Using"))
            .Or(KarelWait.GetParser().WithErrorContext("Wait"))
            .Or(KarelWhile.GetParser().WithErrorContext("While"))
            .Or(KarelWrite.GetParser().WithErrorContext("Write"))
            .IgnoreComments()
            .WithPos();

}

public abstract record KarelExpression : WithPosition, IKarelParser<KarelExpression>
{
    public static readonly Parser<KarelExpression> ExprRef = Parse.Ref(() => Expression);

    private static readonly Parser<KarelExpression> Primary
        = KarelFunctionCall.GetParser().WithErrorContext("Call Expression")
            .Or(KarelValue.GetParser().WithErrorContext("Value Expression"))
            .Or(ExprRef.BetweenParen());

    private static readonly Parser<KarelFactorExpression> Not
        = from kw in KarelCommon.Keyword("NOT")
          from expr in Primary
          select new KarelNotExpression(expr);

    // Unary minus on any primary (variable, call, parenthesised expression).
    // Negative numeric literals are already handled inside KarelInteger/KarelReal,
    // so this is tried after Primary to leave those AST shapes unchanged and only
    // pick up the cases the literal parsers reject (e.g. -axis_pos).
    private static readonly Parser<KarelFactorExpression> Negate
        = from op in KarelCommon.Minus
          from expr in Primary
          select new KarelUnaryMinus(expr);

    private static readonly Parser<KarelExpression> FactorExpr =
        Parse.ChainOperator(KarelPositionOperatorParser.Parser(),
            Not.Or(Primary).Or(Negate),
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

public sealed record KarelUnaryMinus(KarelExpression Expr)
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

    private static readonly Parser<string> Exponential =
        from leading in Parse.Chars('-', '+').Optional()
        from num in Parse.Decimal
        from exp in Parse.Chars('e', 'E')
        from expSign in Parse.Chars('-', '+').Optional()
        from exponent in Parse.Number
        select (leading.GetOrDefault() + num + exp + expSign.GetOrDefault() + exponent).Replace("\0", "");

    private static readonly Parser<string> Decimal =
        from leading in Parse.Chars('-', '+').Optional()
        from num in Parse.Number
        from dot in Parse.Char('.')
        from frac in Parse.Number.Optional()
        select (leading.GetOrDefault() + num + dot + frac.GetOrElse(string.Empty)).Replace("\0", "");

    public new static Parser<KarelValue> GetParser()
        => from num in Exponential.Or(Decimal)
           select new KarelReal(float.Parse(num, NumberStyles.Float));
}

public sealed record KarelBool(bool Value) : KarelValue, IKarelParser<KarelValue>
{
    public override string ToString()
        => Value.ToString();

    public new static Parser<KarelValue> GetParser()
        => KarelCommon.Keyword("TRUE").Return(new KarelBool(true))
            .Or(KarelCommon.Keyword("FALSE").Return(new KarelBool(false)));
}
