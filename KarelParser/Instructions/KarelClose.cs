using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelCloseFile(KarelVariableAcess File) : KarelInstruction, IKarelParser<KarelInstruction>
{
    public new static Parser<KarelInstruction> GetParser()
        => from kw in KarelCommon.Keyword("CLOSE")
           from kww in KarelCommon.Keyword("FILE")
           from file in KarelVariableAcess.GetParser().WithPos()
           select new KarelCloseFile(file);
}

public sealed record KarelCloseHand(KarelExpression Hand) : KarelInstruction, IKarelParser<KarelInstruction>
{
    public new static Parser<KarelInstruction> GetParser()
        => from kw in KarelCommon.Keyword("CLOSE")
           from kww in KarelCommon.Keyword("HAND")
           from hand in KarelExpression.GetParser().WithPos()
           select new KarelCloseHand(hand);
}
