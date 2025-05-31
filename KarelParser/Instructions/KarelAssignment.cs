using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelAssignment(KarelVariableAcess Variable, KarelExpression Expr)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from variable in KarelVariableAcess.GetParser().WithPos()
           from sep in KarelCommon.Keyword("=")
           from expr in KarelExpression.GetParser()
           select new KarelAssignment(variable, expr);
}
