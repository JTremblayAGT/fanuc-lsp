using ParserUtils;
using Sprache;

namespace KarelParser;

public sealed record KarelConstant(string Identifier, KarelValue Value) : WithPosition, IKarelParser<KarelConstant>
{
    private static Parser<KarelConstant> InternalParser()
        => from ident in KarelCommon.Identifier
            from sep in ParserUtils.ParserExtensions.Keyword("=")
            from val in KarelValue.GetParser()
            select new KarelConstant(ident, val);

    public new static Parser<KarelConstant> GetParser()
        => InternalParser()
            .WithPosition()
            .Select(result => result.Value with
            {
                Start = result.Start, End = result.End
            });
}