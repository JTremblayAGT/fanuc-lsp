using ParserUtils;
using Sprache;

namespace KarelParser;

public record KarelDeclaration : WithPosition, IKarelParser<KarelDeclaration>
{
    private static Parser<KarelDeclaration> InternalParser()
        => KarelTypeDeclaration.GetParser()
            .Or(KarelVariableDeclaration.GetParser())
            .Or(KarelConstantDeclaration.GetParser());

    public static Parser<KarelDeclaration> GetParser()
        => InternalParser()
            .WithPosition()
            .Select(result => result.Value with
            {
                Start = result.Start, End = result.End
            });
}

public sealed record KarelTypeDeclaration(KarelType Type)
    : KarelDeclaration, IKarelParser<KarelDeclaration>
{
    public new static Parser<KarelDeclaration> GetParser()
        => throw new NotImplementedException();
}

public sealed record KarelVariableDeclaration(List<KarelVariable> Variable)
    : KarelDeclaration, IKarelParser<KarelDeclaration>
{
    public new static Parser<KarelDeclaration> GetParser()
        => from kw in ParserUtils.ParserExtensions.Keyword("VAR")
            from variables in KarelVariable.GetParser().AtLeastOnce()
            select new KarelVariableDeclaration(variables.SelectMany(var => var).ToList());
}

public sealed record KarelConstantDeclaration(List<KarelConstant> Constants)
    : KarelDeclaration, IKarelParser<KarelDeclaration>
{
    public new static Parser<KarelDeclaration> GetParser()
        => from kw in ParserUtils.ParserExtensions.Keyword("CONST")
            from constants in KarelConstant.GetParser().AtLeastOnce()
            select new KarelConstantDeclaration(constants.ToList());
}