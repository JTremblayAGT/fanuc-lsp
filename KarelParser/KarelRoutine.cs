using ParserUtils;
using Sprache;

namespace KarelParser;

public sealed record KarelRoutine(
    string Identifier,
    List<KarelArg> Args,
    KarelDataType? ReturnType,
    List<KarelInstruction> Instructions)
    : WithPosition, IKarelParser<KarelRoutine>
{
    private static Parser<KarelRoutine> InternalParser()
        => from kw in KarelCommon.Keyword("ROUTINE")
           from ident in KarelCommon.Identifier
           from args in KarelArg.GetParser()
               .Many()
               .Select(args => args.SelectMany(lst => lst))
               .BetweenParen()
           from ret in KarelCommon.Keyword(":").Then(_ => KarelDataType.GetParser()).Optional()
           from begin in KarelCommon.Keyword("BEGIN")
           from instructions in KarelInstruction.GetParser().Many()
           from end in KarelCommon.Keyword("END").Then(_ => KarelCommon.Identifier)
           select new KarelRoutine(ident, args.ToList(), ret.GetOrElse(null), instructions.ToList());

    public static Parser<KarelRoutine> GetParser()
        => InternalParser()
            .WithPosition()
            .Select(result => result.Value with
            {
                Start = result.Start,
                End = result.End
            });
}

public sealed record KarelArg(string Identifier, KarelDataType Type)
    : WithPosition
{
    public static Parser<List<KarelArg>> GetParser()
        => from idents in KarelCommon.Identifier.WithPosition().DelimitedBy(KarelCommon.Keyword(","), 1, int.MaxValue)
           from sep in KarelCommon.Keyword(":")
           from type in KarelDataType.GetParser()
           select idents.Select(ident => new KarelArg(ident.Value, type)
           {
               Start = ident.Start,
               End = ident.End
           }).ToList();
}

