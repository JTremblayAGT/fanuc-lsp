using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelCall(string Identifier, List<KarelExpression> Args)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("CALL").Optional()
           from ident in KarelCommon.Identifier
           from args in KarelExpression.GetParser()
               .DelimitedBy(KarelCommon.Keyword(","), 1, null)
               .BetweenParen()
               .Optional()
           select new KarelCall(ident, args.GetOrElse([]).ToList());
}
