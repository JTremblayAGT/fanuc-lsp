using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelIfThenElse(KarelExpression Expr, List<KarelStatement> Body, List<KarelStatement> Else)
    : KarelStatement, IKarelParser<KarelStatement>
{
    private static readonly Parser<KarelStatement> Internal =
        from kw in KarelCommon.Keyword("IF")
        from expr in KarelExpression.GetParser()
        from kww in KarelCommon.Keyword("THEN")
        from body in KarelCommon.ParseStatements(["ELSE"]).WithErrorContext("IfBody")
        from kwww in KarelCommon.Keyword("ELSE").IgnoreComments()
        from else_ in KarelCommon.ParseStatements(["ENDIF"]).WithErrorContext("ElseBody")
            //from brk in KarelCommon.LineBreak
        from kwwww in KarelCommon.Keyword("ENDIF").IgnoreComments()
        select new KarelIfThenElse(expr, body.ToList(), else_.ToList());

    public new static Parser<KarelStatement> GetParser()
        => Internal;
}

public sealed record KarelIfThen(KarelExpression Expr, List<KarelStatement> Body)
    : KarelStatement, IKarelParser<KarelStatement>
{
    private static readonly Parser<KarelStatement> Internal =
        from kw in KarelCommon.Keyword("IF")
        from expr in KarelExpression.GetParser()
        from kww in KarelCommon.Keyword("THEN")
        from body in KarelCommon.ParseStatements(["ENDIF"]).WithErrorContext("IfBody")
        from kwwww in KarelCommon.Keyword("ENDIF").IgnoreComments()
        select new KarelIfThen(expr, body.ToList());

    public new static Parser<KarelStatement> GetParser()
        => Internal;
}
