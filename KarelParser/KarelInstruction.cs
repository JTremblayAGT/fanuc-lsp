using ParserUtils;
using Sprache;

namespace KarelParser;

public record KarelStatement() : WithPosition, IKarelParser<KarelStatement>
{
    public static Parser<KarelStatement> GetParser()
        => throw new NotImplementedException();
}

public record KarelExpression() : WithPosition, IKarelParser<KarelExpression>
{
    public static Parser<KarelExpression> GetParser()
        => throw new NotImplementedException();
}

