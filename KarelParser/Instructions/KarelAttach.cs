using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelAttach : KarelInstruction, IKarelParser<KarelInstruction>
{
    public new static Parser<KarelInstruction> GetParser()
        => KarelCommon.Keyword("ATTACH").Return(new KarelAttach());
}
