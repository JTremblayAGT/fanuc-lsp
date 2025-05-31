using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelRead(KarelVariableAcess? Variable, List<KarelItem> Items)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from variable in KarelVariableAcess.GetParser().WithPos().Optional()
           from items in KarelItem.GetParser().AtLeastOnce().BetweenParen()
           select new KarelRead(variable.GetOrElse(null), items.ToList());
}

public sealed record KarelWrite(KarelVariableAcess? Variable, List<KarelItem> Items)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from variable in KarelVariableAcess.GetParser().WithPos().Optional()
           from items in KarelItem.GetParser().AtLeastOnce().BetweenParen()
           select new KarelWrite(variable.GetOrElse(null), items.ToList());
}

public record KarelItem : IKarelParser<KarelItem>
{
    protected static Parser<List<KarelExpression>> Items()
        => (from kww in KarelCommon.Keyword("::")
            from expr in KarelExpression.GetParser()
            select expr).Repeat(1, 2).Select(lst => lst.ToList());

    public static Parser<KarelItem> GetParser()
        => KarelReadItemVariable.GetParser()
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

public record KarelReadItemVariable(KarelVariableAcess Variable, List<KarelExpression> FormatSpecs)
    : KarelItem, IKarelParser<KarelItem>
{
    public new static Parser<KarelItem> GetParser()
        => from variable in KarelVariableAcess.GetParser().WithPos()
           from items in Items()
           select new KarelReadItemVariable(variable, items);
}


