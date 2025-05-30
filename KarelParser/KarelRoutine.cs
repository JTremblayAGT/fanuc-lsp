using ParserUtils;
using Sprache;

namespace KarelParser;

public sealed record KarelRoutine() : WithPosition, IKarelParser<KarelRoutine>
{
    public static Parser<KarelRoutine> GetParser()
        => throw new NotImplementedException();
}