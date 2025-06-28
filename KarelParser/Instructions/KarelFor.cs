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
    private static readonly Parser<KarelStatement> Internal =
        from kw in KarelCommon.Keyword("FOR")
        from ident in KarelCommon.Identifier
        from sep in KarelCommon.Keyword("=")
        from initial in KarelExpression.GetParser()
        from dir in ForDirectionParser.Parser()
        from target in KarelExpression.GetParser()
        from kww in KarelCommon.Keyword("DO")
        from body in KarelCommon.ParseStatements(["ENDFOR"])
        from kwww in KarelCommon.Keyword("ENDFOR")
        select new KarelFor(ident, initial, target, dir, body.ToList());

    public new static Parser<KarelStatement> GetParser()
        => Internal.WithErrorContext("FOR");
}

public sealed record KarelRepeat(List<KarelStatement> Body, KarelExpression Expr)
    : KarelStatement, IKarelParser<KarelStatement>
{
    private static readonly Parser<KarelStatement> Internal =
        from kw in KarelCommon.Keyword("REPEAT")
        from body in KarelCommon.ParseStatements(["UNTIL"])
        from kww in KarelCommon.Keyword("UNTIL")
        from expr in KarelExpression.GetParser()
        select new KarelRepeat(body.ToList(), expr);

    public new static Parser<KarelStatement> GetParser()
        => Internal.WithErrorContext("REPEAT");
}
