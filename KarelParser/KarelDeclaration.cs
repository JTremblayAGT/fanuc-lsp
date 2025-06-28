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
        => InternalParser().WithPos();
}

public sealed record KarelTypeDeclaration(List<KarelType> Type)
    : KarelDeclaration, IKarelParser<KarelDeclaration>
{
    public new static Parser<KarelDeclaration> GetParser()
        => from kw in KarelCommon.Keyword("TYPE")
           from types in KarelType.GetParser().IgnoreComments().AtLeastOnce()
           select new KarelTypeDeclaration(types.ToList());
}

public sealed record KarelVariableDeclaration(List<KarelVariable> Variable)
    : KarelDeclaration, IKarelParser<KarelDeclaration>
{
    public new static Parser<KarelDeclaration> GetParser()
        => from kw in KarelCommon.Keyword("VAR")
           from variables in KarelVariable.GetParser().IgnoreComments().AtLeastOnce()
           select new KarelVariableDeclaration(variables.SelectMany(var => var).ToList());
}

public sealed record KarelConstantDeclaration(List<KarelConstant> Constants)
    : KarelDeclaration, IKarelParser<KarelDeclaration>
{
    public new static Parser<KarelDeclaration> GetParser()
        => from kw in KarelCommon.Keyword("CONST")
           from constants in KarelConstant.GetParser().IgnoreComments().AtLeastOnce()
           select new KarelConstantDeclaration(constants.ToList());
}
