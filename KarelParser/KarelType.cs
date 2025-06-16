using ParserUtils;
using Sprache;

namespace KarelParser;

public sealed record KarelType(string Identifier, KarelUserType Type, string FromProgram)
    : WithPosition, IKarelParser<KarelType>
{
    private static Parser<KarelType> InternalParser()
        => from ident in KarelCommon.Identifier
           from program in KarelCommon.Keyword("FROM")
               .Then(_ => KarelCommon.Identifier).Optional()
           from sep in KarelCommon.Keyword("=")
           from userType in KarelUserType.GetParser()
           select new KarelType(ident, userType, program.GetOrElse(string.Empty));

    public static Parser<KarelType> GetParser()
        => InternalParser().WithPos();
}

public record KarelUserType : WithPosition, IKarelParser<KarelUserType>
{
    private static Parser<KarelUserType> InternalParser()
        => KarelDataType.GetParser()
            .Or(KarelStructure.GetParser());

    public static Parser<KarelUserType> GetParser()
        => InternalParser().WithPos();
}

public record KarelDataType
    : KarelUserType, IKarelParser<KarelDataType>
{
    private static Parser<KarelDataType> InternalParser()
        => KarelTypeString.GetParser()
            .Or(KarelTypeArray.GetParser())
            .Or(KarelTypePosition.GetParser())
            .Or(KarelTypeName.GetParser())
            /*.Or(KarelTypePath.GetParser())*/;

    public new static Parser<KarelDataType> GetParser()
        => InternalParser().WithPos();
}

public sealed record KarelTypeName(string Identifier)
    : KarelDataType, IKarelParser<KarelDataType>
{
    public new static Parser<KarelDataType> GetParser()
        => KarelCommon.Identifier.Or(KarelCommon.Reserved).Select(ident => new KarelTypeName(ident));
}

public sealed record KarelTypeString(int Size)
    : KarelDataType, IKarelParser<KarelDataType>
{
    public new static Parser<KarelDataType> GetParser()
        => from kw in KarelCommon.Keyword("STRING")
           from size in Parse.Number.BetweenBrackets().Select(int.Parse)
           select new KarelTypeString(size);
}

public sealed record KarelTypeArray(List<int> Size, KarelDataType Type)
    : KarelDataType, IKarelParser<KarelDataType>
{
    public new static Parser<KarelDataType> GetParser()
        => from kw in KarelCommon.Keyword("ARRAY")
           from size in Parse.Number.Select(int.Parse)
            .DelimitedBy(Parse.Char(','), 1, int.MaxValue)
            .BetweenBrackets()
           from sep in KarelCommon.Keyword("OF")
           from type in Parse.Ref(() => KarelDataType.GetParser())
           select new KarelTypeArray(size.ToList(), (KarelDataType)type);
}

public sealed record KarelTypePosition(string PosType, int Group)
    : KarelDataType, IKarelParser<KarelDataType>
{
    public new static Parser<KarelDataType> GetParser()
        => from posType in KarelCommon.Identifier
           from sep in KarelCommon.Keyword("IN")
           from grp in KarelCommon.Keyword("GROUP")
           from num in Parse.Number.Select(int.Parse).BetweenBrackets()
           select new KarelTypePosition(posType, num);
}

public sealed record KarelTypePath
    : KarelDataType, IKarelParser<KarelDataType>
{
    public new static Parser<KarelDataType> GetParser()
        => input => Result.Failure<KarelDataType>(input, "Karel path not implemented", []);
}

public sealed record KarelStructure(string Identifier, List<KarelField> Fields)
    : KarelUserType, IKarelParser<KarelUserType>
{
    private static Parser<KarelStructure> InternalParser()
        => from ident in KarelCommon.Identifier
           from sep in KarelCommon.Keyword("=")
           from structOpen in KarelCommon.Keyword("STRUCTURE")
           from fields in KarelField.GetParser().DelimitedBy(KarelCommon.LineBreak, 1, null)
           from brk in KarelCommon.LineBreak
           from structClose in KarelCommon.Keyword("ENDSTRUCTURE")
           select new KarelStructure(ident, fields.ToList());

    public new static Parser<KarelUserType> GetParser()
        => InternalParser().WithPos();
}

public record KarelField(string Identifier, KarelDataType Type)
    : WithPosition, IKarelParser<KarelField>
{
    private static Parser<KarelField> InternalParser()
        => from ident in KarelCommon.Identifier
           from sep in KarelCommon.Keyword(":")
           from type in Parse.Ref(() => KarelDataType.GetParser())
           select new KarelField(ident, (KarelDataType)type);

    public static Parser<KarelField> GetParser()
        => InternalParser().WithPos();
}
