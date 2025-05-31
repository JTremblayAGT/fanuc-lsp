using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelAttach : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => KarelCommon.Keyword("ATTACH").Return(new KarelAttach());
}
