using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelSelect(KarelExpression Expr, List<KarelCase> Cases, KarelCase? ElseCase)
    : KarelStatement, IKarelParser<KarelStatement>
{
    private static readonly Parser<KarelStatement> Internal =
        from kw in KarelCommon.Keyword("SELECT")
        from expr in KarelExpression.GetParser()
        from kww in KarelCommon.Keyword("OF")
        from cases in KarelValueCase.GetParser().WithErrorContext("CASE").XMany()
        from elseCase in KarelElseCase.GetParser().WithErrorContext("ELSE CASE").Optional()
        from kwww in KarelCommon.Keyword("ENDSELECT").IgnoreComments()
        select new KarelSelect(expr, cases.ToList(), elseCase.GetOrElse(null));


    public new static Parser<KarelStatement> GetParser()
        => Internal.WithErrorContext("SELECT");
}

public record KarelCase : IKarelParser<KarelCase>
{
    public static Parser<KarelCase> GetParser()
        => KarelValueCase.GetParser()
            .Or(KarelElseCase.GetParser());
}

public sealed record KarelValueCase(List<KarelValue> Values, List<KarelStatement> Body) : KarelCase, IKarelParser<KarelCase>
{
    public new static Parser<KarelCase> GetParser()
        => from kw in KarelCommon.Keyword("CASE")
           from values in KarelValue.GetParser().DelimitedBy(KarelCommon.Keyword(","), 1, null).BetweenParen()
           from sep in KarelCommon.Keyword(":")
           from body in KarelCommon.ParseStatements(["CASE", "ELSE:", "ENDSELECT"])
           select new KarelValueCase(values.ToList(), body.ToList());
}

public sealed record KarelElseCase(List<KarelStatement> Body) : KarelCase, IKarelParser<KarelCase>
{
    public new static Parser<KarelCase> GetParser()
        => from kw in KarelCommon.Keyword("ELSE")
           from sep in KarelCommon.Keyword(":")
           from body in KarelCommon.ParseStatements(["ENDSELECT"])
           select new KarelElseCase(body.ToList());
}
