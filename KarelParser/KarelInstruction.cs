using ParserUtils;
using Sprache;

namespace KarelParser;

public record KarelInstruction() : WithPosition, IKarelParser<KarelInstruction>
{
    public static Parser<KarelInstruction> GetParser()
        => throw new NotImplementedException();
}

public record KarelExpression() : WithPosition, IKarelParser<KarelExpression>
{
    public static Parser<KarelExpression> GetParser()
        => throw new NotImplementedException();
}

