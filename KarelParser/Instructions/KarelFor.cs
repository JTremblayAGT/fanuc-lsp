using Sprache;

namespace KarelParser.Instructions;

public enum ForDirection
{
    Up,
    Down
}

public struct ForDirectionParser
{
    public static Parser<ForDirection> Parser()
        => KarelCommon.Keyword("TO").Return(ForDirection.Up)
            .Or(KarelCommon.Keyword("DOWNTO").Return(ForDirection.Down));
}

public sealed record KarelFor(
        string CountVariable,
        KarelExpression InitialValue,
        KarelExpression TargetValue,
        ForDirection Direction,
        List<KarelStatement> Body)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("FOR")
           from ident in KarelCommon.Identifier
           from sep in KarelCommon.Keyword("=")
           from initial in KarelExpression.GetParser()
           from dir in ForDirectionParser.Parser()
           from target in KarelExpression.GetParser()
           from kww in KarelCommon.Keyword("DO")
           from body in Parse.Ref(() => KarelStatement.GetParser()).Many()
           from brk in KarelCommon.LineBreak
           from kwww in KarelCommon.Keyword("ENDFOR")
           select new KarelFor(ident, initial, target, dir, body.ToList());
}

public sealed record KarelRepeat(List<KarelStatement> Body, KarelExpression Expr)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("REPEAT")
           from body in Parse.Ref(() => KarelStatement.GetParser()).Many()
           from brk in KarelCommon.LineBreak
           from kww in KarelCommon.Keyword("UNTIL")
           from expr in KarelExpression.GetParser()
           select new KarelRepeat(body.ToList(), expr);
}
