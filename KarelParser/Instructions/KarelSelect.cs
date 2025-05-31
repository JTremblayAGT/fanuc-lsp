using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelSelect(KarelExpression Expr, List<KarelCase> Cases)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("SELECT")
           from expr in KarelExpression.GetParser()
           from kww in KarelCommon.Keyword("OF")
           from cases in KarelCase.GetParser().Repeat(1, 2)
           select new KarelSelect(expr, cases.ToList());
}

public record KarelCase : IKarelParser<KarelCase>
{
    public static Parser<KarelCase> GetParser()
        => KarelValueCase.GetParser()
            .Or(KarelElseCase.GetParser());
}

public sealed record KarelValueCase(List<KarelInteger> Values, List<KarelStatement> Body) : KarelCase, IKarelParser<KarelCase>
{
    public new static Parser<KarelCase> GetParser()
        => from kw in KarelCommon.Keyword("CASE")
           from values in KarelInteger.GetParser().DelimitedBy(KarelCommon.Keyword(","), 1, null).BetweenParen()
           from sep in KarelCommon.Keyword(":")
           from body in Parse.Ref(() => KarelStatement.GetParser()).Many()
           from brk in KarelCommon.LineBreak
           select new KarelValueCase(values.OfType<KarelInteger>().ToList(), body.ToList());
}

public sealed record KarelElseCase(List<KarelStatement> Body) : KarelCase, IKarelParser<KarelCase>
{
    public new static Parser<KarelCase> GetParser()
        => from kw in KarelCommon.Keyword("ELSE")
           from sep in KarelCommon.Keyword(":")
           from body in Parse.Ref(() => KarelStatement.GetParser()).Many()
           from brk in KarelCommon.LineBreak
           select new KarelElseCase(body.ToList());
}
