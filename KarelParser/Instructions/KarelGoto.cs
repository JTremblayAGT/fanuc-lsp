using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelGoto(string Identifier) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("GOTO")
                .Or(KarelCommon.Keyword("GO").Then(_ => KarelCommon.Keyword("TO")))
           from ident in KarelCommon.Identifier
           select new KarelGoto(ident);
}
