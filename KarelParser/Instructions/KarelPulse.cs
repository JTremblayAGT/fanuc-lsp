using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelPulse() : KarelStatement, IKarelParser<KarelStatement>
{
    // TODO:
    public new static Parser<KarelStatement> GetParser()
        => Parse.Return(new KarelPulse());
}
