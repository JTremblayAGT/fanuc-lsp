using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelCancel(List<int> Groups) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("CANCEL")
           from groups in (from kww in KarelCommon.Keyword("GROUP")
                           from groups in Parse.Number.Select(int.Parse).AtLeastOnce().BetweenBrackets()
                           select groups).Optional()
           select new KarelCancel(groups.GetOrElse([]).ToList());
}

public sealed record KarelCancelFile(KarelVariableAcess File) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("CANCEL")
           from kww in KarelCommon.Keyword("FILE")
           from file in KarelVariableAcess.GetParser().WithPos()
           select new KarelCancelFile(file);
}
