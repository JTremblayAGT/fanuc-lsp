using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelHold(List<int> Groups) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("HOLD")
           from groups in (from kww in KarelCommon.Keyword("GROUP")
                           from groups in Parse.Number.Select(int.Parse)
                                .DelimitedBy(KarelCommon.Keyword(","), 1, null)
                                .BetweenBrackets()
                           select groups).Optional()
           select new KarelHold(groups.GetOrElse([]).ToList());
}

public sealed record KarelResume(List<int> Groups) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("RESUME")
           from groups in (from kww in KarelCommon.Keyword("GROUP")
                           from groups in Parse.Number.Select(int.Parse)
                                .DelimitedBy(KarelCommon.Keyword(","), 1, null)
                                .BetweenBrackets()
                           select groups).Optional()
           select new KarelResume(groups.GetOrElse([]).ToList());
}

public sealed record KarelRelease : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelRelease> GetParser()
        => KarelCommon.Keyword("RELEASE").Return(new KarelRelease());
}

public sealed record KarelUnhold(List<int> Groups) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("HOLD")
           from groups in (from kww in KarelCommon.Keyword("GROUP")
                           from groups in Parse.Number.Select(int.Parse)
                                .DelimitedBy(KarelCommon.Keyword(","), 1, null)
                                .BetweenBrackets()
                           select groups).Optional()
           select new KarelUnhold(groups.GetOrElse([]).ToList());
}
