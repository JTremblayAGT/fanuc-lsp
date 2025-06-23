using Sprache;
using KarelParser.Conditions;

namespace KarelParser.Instructions;

public sealed record KarelWait(KarelGlobalCondition Condition)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("WAIT")
           from kww in KarelCommon.Keyword("FOR")
           from cond in ParseCompoundCondition()
           select new KarelWait(cond);
           
    private static Parser<KarelGlobalCondition> ParseCompoundCondition()
        => KarelOrCondition.OrChain(KarelGlobalCondition.GetParser())
           .Or(KarelAndCondition.AndChain(KarelGlobalCondition.GetParser()));
}
