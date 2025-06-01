using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelPulse(string Port, KarelExpression Index, KarelExpression Time, bool NoWait) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("PULSE")
           from port in KarelCommon.Keyword("DOUT").Or(KarelCommon.Keyword("RDO"))
           from index in KarelExpression.GetParser().BetweenBrackets()
           from kww in KarelCommon.Keyword("FOR")
           from time in KarelExpression.GetParser()
           from nowait in KarelCommon.Keyword("NOWAIT").Optional()
           select new KarelPulse(port, index, time, nowait.IsDefined);
}
