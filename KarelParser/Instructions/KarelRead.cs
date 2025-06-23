using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelRead(KarelVariableAccess? Variable, List<KarelItem> Items)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("READ")
           from variable in KarelVariableAccess.GetParser().WithPos().Optional()
           from items in KarelItem.GetParser().DelimitedBy(KarelCommon.Keyword(",")).BetweenParen()
           select new KarelRead(variable.GetOrElse(null), items.ToList());
}

public sealed record KarelWrite(KarelVariableAccess? Variable, List<KarelItem> Items)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("WRITE")
           from variable in KarelVariableAccess.GetParser().WithPos().Optional()
           from items in KarelItem.GetParser().DelimitedBy(KarelCommon.Keyword(",")).BetweenParen()
           select new KarelWrite(variable.GetOrElse(null), items.ToList());
}

public record KarelItem : IKarelParser<KarelItem>
{
    protected static Parser<List<KarelExpression>> Items()
        => (from kww in KarelCommon.Keyword("::")
            from expr in KarelExpression.GetParser()
            select expr).Repeat(1, 2).Optional().Select(lst => lst.GetOrElse([]).ToList());

    public static Parser<KarelItem> GetParser()
        => KarelReadItemExpr.GetParser()
            .Or(KarelReadItemCR.GetParser());

}

public record KarelReadItemCR(List<KarelExpression> FormatSpecs)
    : KarelItem, IKarelParser<KarelItem>
{
    public new static Parser<KarelItem> GetParser()
        => from kw in KarelCommon.Keyword("CR")
           from items in Items()
           select new KarelReadItemCR(items);
}

public record KarelReadItemExpr(KarelExpression Expression, List<KarelExpression> FormatSpecs)
    : KarelItem, IKarelParser<KarelItem>
{
    public new static Parser<KarelItem> GetParser()
        => from variable in KarelExpression.GetParser() 
           from items in Items()
           select new KarelReadItemExpr(variable, items);
}


