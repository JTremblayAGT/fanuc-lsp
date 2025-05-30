using ParserUtils;
using Sprache;

namespace KarelParser;

public record KarelInstruction() : WithPosition, IKarelParser<KarelInstruction>
{
    public static Parser<KarelInstruction> GetParser()
        => throw new NotImplementedException();
}