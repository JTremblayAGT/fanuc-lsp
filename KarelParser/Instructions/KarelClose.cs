using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelCloseFile(KarelVariableAcess File) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("CLOSE")
           from kww in KarelCommon.Keyword("FILE")
           from file in KarelVariableAcess.GetParser().WithPos()
           select new KarelCloseFile(file);
}

public sealed record KarelCloseHand(KarelExpression Hand) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("CLOSE")
           from kww in KarelCommon.Keyword("HAND")
           from hand in KarelExpression.GetParser().WithPos()
           select new KarelCloseHand(hand);
}
