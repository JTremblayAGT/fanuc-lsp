using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelAssignment(KarelVariableAcess Variable, KarelExpression Expr)
    : KarelInstruction, IKarelParser<KarelInstruction>
{
    public new static Parser<KarelInstruction> GetParser()
        => from variable in KarelVariableAcess.GetParser().WithPos()
           from sep in KarelCommon.Keyword("=")
           from expr in KarelExpression.GetParser()
           select new KarelAssignment(variable, expr);
}
