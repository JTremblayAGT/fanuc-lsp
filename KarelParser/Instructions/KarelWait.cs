using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelWait(KarelGlobalCondition Condition)
    : KarelStatement, IKarelParser<KarelStatement>
{
    // TODO: need to support AND & OR
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("WAIT")
           from kww in KarelCommon.Keyword("FOR")
           from cond in KarelGlobalCondition.GetParser()
           select new KarelWait(cond);
}
