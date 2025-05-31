using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelCall(string Identifier, List<KarelExpression> Args)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelCall> GetParser()
        => from kw in KarelCommon.Keyword("CALL")
           from ident in KarelCommon.Identifier
           from args in KarelExpression.GetParser().AtLeastOnce().BetweenParen().Optional()
           select new KarelCall(ident, args.GetOrElse([]).ToList());
}
