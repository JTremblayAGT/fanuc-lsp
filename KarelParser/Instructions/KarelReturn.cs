using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelReturn(KarelExpression Expr)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("RETURN")
           from expr in KarelExpression.GetParser().BetweenParen()
           select new KarelReturn(expr);
}
