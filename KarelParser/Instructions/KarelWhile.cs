using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelWhile(KarelExpression Expr, List<KarelStatement> Body)
    : KarelStatement, IKarelParser<KarelStatement>
{
    private static readonly Parser<KarelStatement> Internal =
        from kw in KarelCommon.Keyword("WHILE")
        from expr in KarelExpression.GetParser()
        from kww in KarelCommon.Keyword("DO")
        from body in KarelCommon.ParseStatements(["ENDWHILE"])
        //from brk in KarelCommon.LineBreak
        from kwww in KarelCommon.Keyword("ENDWHILE")
        select new KarelWhile(expr, body.ToList());

    public new static Parser<KarelStatement> GetParser()
        => Internal.WithErrorContext("WHILE");
}
