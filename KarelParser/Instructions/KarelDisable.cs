using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelDisable(KarelExpression Expr) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("DISABLE")
           from kww in KarelCommon.Keyword("CONDITION")
           from expr in KarelExpression.GetParser().BetweenBrackets()
           select new KarelDisable(expr);
}

public sealed record KarelEnable(KarelExpression Expr) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("ENABLE")
           from kww in KarelCommon.Keyword("CONDITION")
           from expr in KarelExpression.GetParser().BetweenBrackets()
           select new KarelEnable(expr);
}

public sealed record KarelPurge(KarelExpression Expr) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("PURGE")
           from kww in KarelCommon.Keyword("CONDITION")
           from expr in KarelExpression.GetParser().BetweenBrackets()
           select new KarelPurge(expr);
}
