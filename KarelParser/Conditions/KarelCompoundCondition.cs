using Sprache;
using System.Collections.Generic;

namespace KarelParser.Conditions;

public sealed record KarelAndCondition(IReadOnlyList<KarelGlobalCondition> Conditions)
    : KarelGlobalCondition
{
    public static Parser<KarelGlobalCondition> AndChain(Parser<KarelGlobalCondition> conditionParser)
        => from first in conditionParser
           from rest in 
                (from and in KarelCommon.Keyword("AND").Token()
                 from cond in conditionParser
                 select cond).Many()
           select rest.Any() 
                ? new KarelAndCondition(new[] { first }.Concat(rest).ToList())
                : first;
}

public sealed record KarelOrCondition(IReadOnlyList<KarelGlobalCondition> Conditions)
    : KarelGlobalCondition
{
    public static Parser<KarelGlobalCondition> OrChain(Parser<KarelGlobalCondition> conditionParser)
        => from first in conditionParser
           from rest in 
                (from or in KarelCommon.Keyword("OR").Token()
                 from cond in conditionParser
                 select cond).Many()
           select rest.Any() 
                ? new KarelOrCondition(new[] { first }.Concat(rest).ToList())
                : first;
}