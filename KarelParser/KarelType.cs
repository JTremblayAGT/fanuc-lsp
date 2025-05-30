using ParserUtils;
using Sprache;

namespace KarelParser;

public sealed record KarelType : WithPosition, IKarelParser<KarelType>
{
    public static Parser<KarelType> GetParser()
        => throw new NotImplementedException();
}