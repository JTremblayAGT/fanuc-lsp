using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelOpenFile(KarelVariableAcess File, KarelExpression Usage, KarelExpression Spec) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("OPEN")
           from kww in KarelCommon.Keyword("FILE")
           from file in KarelVariableAcess.GetParser().WithPos()
           from open in KarelCommon.Keyword("(")
           from usage in KarelExpression.GetParser()
           from sep in KarelCommon.Keyword(",")
           from spec in KarelExpression.GetParser()
           from close in KarelCommon.Keyword(")")
           select new KarelOpenFile(file, usage, spec);
}

public sealed record KarelOpenHand(KarelExpression Hand) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("OPEN")
           from kww in KarelCommon.Keyword("HAND")
           from hand in KarelExpression.GetParser().WithPos()
           select new KarelOpenHand(hand);
}

public sealed record KarelRelaxHand(KarelExpression Hand) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("RELAX")
           from kww in KarelCommon.Keyword("HAND")
           from hand in KarelExpression.GetParser().WithPos()
           select new KarelRelaxHand(hand);
}
