using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelWait(KarelGlobalCondition Condition)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => throw new NotImplementedException();
}
