using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelDelay(KarelExpression Expr) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("DELAY")
           from expr in KarelExpression.GetParser()
           select new KarelDelay(expr);
}
