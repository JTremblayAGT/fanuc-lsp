using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelStop(List<int> Groups) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("STOP")
           from groups in (from kww in KarelCommon.Keyword("GROUP")
                           from groups in Parse.Number.Select(int.Parse)
                                .DelimitedBy(KarelCommon.Keyword(","), 1, null)
                                .BetweenBrackets()
                           select groups).Optional()
           select new KarelUnhold(groups.GetOrElse([]).ToList());
}
