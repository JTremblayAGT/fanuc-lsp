using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelConnectTimer(string Identifier) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("CONNECT")
           from kww in KarelCommon.Keyword("TIMER")
           from kwww in KarelCommon.Keyword("TO")
           from ident in KarelCommon.Identifier
           select new KarelConnectTimer(ident);
}
