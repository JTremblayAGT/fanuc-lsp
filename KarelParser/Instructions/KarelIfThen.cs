using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelIfThen(KarelExpression Expr, List<KarelStatement> Body, KarelElse? Else)
    : KarelStatement, IKarelParser<KarelStatement>
{
    private static readonly Parser<KarelStatement> Internal =
        from kw in KarelCommon.Keyword("IF")
        from expr in KarelExpression.GetParser()
        from kww in KarelCommon.Keyword("THEN")
        from body in KarelCommon.ParseStatements(["ELSE", "ENDIF"])
        from else_ in KarelElse.GetParser().Optional()
        //from brk in KarelCommon.LineBreak
        from kwww in KarelCommon.Keyword("ENDIF")
        select new KarelIfThen(expr, body.ToList(), else_.GetOrElse(null));

    public new static Parser<KarelStatement> GetParser()
        => Internal.WithErrorContext("IF");
}

public sealed record KarelElse(List<KarelStatement> Body) : IKarelParser<KarelElse>
{
    public static Parser<KarelElse> GetParser()
        => from kw in KarelCommon.Keyword("ELSE")
           from body in KarelCommon.ParseStatements(["ENDIF"])
           select new KarelElse(body.ToList());
}

