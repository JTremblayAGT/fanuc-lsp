using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelRead(KarelVariableAcess Variable, List<KarelReadItem> Items)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from variable in KarelVariableAcess.GetParser().WithPos()
           from items in KarelReadItem.GetParser().AtLeastOnce().BetweenParen()
           select new KarelRead(variable, items.ToList());
}

public record KarelReadItem : IKarelParser<KarelReadItem>
{
    protected static Parser<List<KarelExpression>> Items()
        => (from kww in KarelCommon.Keyword("::")
            from expr in KarelExpression.GetParser()
            select expr).Repeat(1, 2).Select(lst => lst.ToList());

    public static Parser<KarelReadItem> GetParser()
        => KarelReadItemVariable.GetParser()
            .Or(KarelReadItemCR.GetParser());

}

public record KarelReadItemCR(List<KarelExpression> FormatSpecs)
    : KarelReadItem, IKarelParser<KarelReadItem>
{
    public new static Parser<KarelReadItem> GetParser()
        => from kw in KarelCommon.Keyword("CR")
           from items in Items()
           select new KarelReadItemCR(items);
}

public record KarelReadItemVariable(KarelVariableAcess Variable, List<KarelExpression> FormatSpecs)
    : KarelReadItem, IKarelParser<KarelReadItem>
{
    public new static Parser<KarelReadItem> GetParser()
        => from variable in KarelVariableAcess.GetParser().WithPos()
           from items in Items()
           select new KarelReadItemVariable(variable, items);
}
