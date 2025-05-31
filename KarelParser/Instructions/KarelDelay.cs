using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelDelay(KarelExpression Expr) : KarelInstruction, IKarelParser<KarelInstruction>
{
    public new static Parser<KarelInstruction> GetParser()
        => from kw in KarelCommon.Keyword("DELAY")
           from expr in KarelExpression.GetParser()
           select new KarelDelay(expr);
}
