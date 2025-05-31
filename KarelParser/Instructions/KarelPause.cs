using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelPause(KarelExpression? TaskNumber) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("PAUSE")
           from taskNum in (from kww in KarelCommon.Keyword("PROGRAM")
                            from taskNum in KarelExpression.GetParser().BetweenBrackets()
                            select taskNum).Optional()
           select new KarelPause(taskNum.GetOrElse(null));
}

