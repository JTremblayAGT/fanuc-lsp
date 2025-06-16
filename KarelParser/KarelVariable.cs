using ParserUtils;
using Sprache;

namespace KarelParser;

public enum KarelVarStorage
{
    Cmos,
    Dram,
    Shadow
}

internal struct KarelVarStorageParser
{
    public static Parser<KarelVarStorage> GetParser()
        => ParserUtils.ParserExtensions.Keyword("CMOS").Return(KarelVarStorage.Cmos)
            .Or(ParserUtils.ParserExtensions.Keyword("DRAM").Return(KarelVarStorage.Dram))
            .Or(ParserUtils.ParserExtensions.Keyword("SHADOW").Return(KarelVarStorage.Shadow));
}

public sealed record KarelVariable(
    string Identifier,
    KarelDataType Type,
    KarelVarStorage Storage,
    string ProgramName
)
    : WithPosition
{
    // multiple variables can be declared at once
    public static Parser<List<KarelVariable>> GetParser()
        => from idents in KarelCommon.Identifier.WithPosition().DelimitedBy(KarelCommon.Keyword(","), 1, int.MaxValue)
            from storage in KarelCommon.Keyword("IN")
                .Then(_ => KarelVarStorageParser.GetParser()).Optional()
            from program in KarelCommon.Keyword("FROM")
                .Then(_ => KarelCommon.Identifier).Optional()
            from sep in KarelCommon.Keyword(":")
            from type in KarelDataType.GetParser()
            select idents.Select(ident => new KarelVariable(ident.Value,
                type,
                storage.GetOrElse(KarelVarStorage.Dram),
                program.GetOrElse(string.Empty))
            {
                Start = ident.Start, End = ident.End
            }).ToList();
}