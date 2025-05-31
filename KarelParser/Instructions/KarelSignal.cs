using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelSignal(KarelExpression Number)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("SIGNAL")
           from kww in KarelCommon.Keyword("EVENT")
           from num in KarelExpression.GetParser().BetweenBrackets()
           select new KarelSignal(num);
}
