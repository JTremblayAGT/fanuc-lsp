using ParserUtils;
using Sprache;

namespace KarelParser;

public sealed record KarelRoutine(
    string Identifier,
    List<KarelArg> Args,
    KarelDataType? ReturnType,
    KarelBody Body)
    : WithPosition, IKarelParser<KarelRoutine>
{
    private static Parser<KarelRoutine> InternalParser()
        => from kw in KarelCommon.Keyword("ROUTINE").IgnoreComments()
           from ident in KarelCommon.Identifier.IgnoreComments()
           from args in KarelArg.GetParser().IgnoreComments()
               .XDelimitedBy(KarelCommon.LineBreak.Or(KarelCommon.Keyword(";")))
               .Select(args => args.SelectMany(lst => lst))
               .BetweenParen().Optional().WithErrorContext("ROUTINE")
           from ret in KarelCommon.Keyword(":").Then(_ => KarelDataType.GetParser()).IgnoreComments().Optional()
           from body in KarelBody.GetParser().IgnoreComments().WithErrorContext($"ROUTINE({ident})")
           select new KarelRoutine(
               ident,
               args.GetOrElse([]).ToList(),
               ret.GetOrElse(null),
               body
           );

    public static Parser<KarelRoutine> GetParser()
        => InternalParser()
            .WithPos();
}

public record KarelBody : WithPosition, IKarelParser<KarelBody>
{
    public static Parser<KarelBody> GetParser()
        => KarelFromBody.GetParser()
            .Or(KarelRoutineBody.GetParser());
}

public sealed record KarelFromBody(string Program) : KarelBody, IKarelParser<KarelBody>
{
    public new static Parser<KarelBody> GetParser()
        => from kw in KarelCommon.Keyword("FROM")
           from prog in KarelCommon.Identifier.WithPosition()
           select new KarelFromBody(prog.Value) { Start = prog.Start, End = prog.End };
}

public sealed record KarelRoutineBody(List<KarelDeclaration> Locals, List<KarelStatement> Body)
    : KarelBody, IKarelParser<KarelBody>
{
    public new static Parser<KarelBody> GetParser()
        => from decls in KarelDeclaration.GetParser().IgnoreComments().XMany()
           from begin in KarelCommon.Keyword("BEGIN").IgnoreComments()
           from instructions in KarelCommon.ParseStatements(["END"])
           from end in KarelCommon.Keyword("END").Then(_ => KarelCommon.Identifier).IgnoreComments()
           select new KarelRoutineBody(decls.ToList(), instructions.ToList());
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

