using Sprache;

namespace KarelParser;

public sealed record KarelProgram(string Name,
    List<KarelTranslatorDirective> TranslatorDirectives,
    List<KarelDeclaration> Declarations,
    List<KarelRoutine> Routines,
    List<KarelStatement> Instructions) : IKarelParser<KarelProgram>
{
    public static Parser<KarelProgram> GetParser()
        => from name in ParserUtils.ParserExtensions.Keyword("PROGRAM").Then(_ => KarelCommon.Identifier)
           from translatorDirectives in KarelTranslatorDirective.GetParser().Many()
           from declarations in KarelDeclaration.GetParser().Many()
           from endName in ParserUtils.ParserExtensions.Keyword("END").Then(_ => KarelCommon.Identifier)
           select new KarelProgram(name, translatorDirectives.ToList(), declarations.ToList(), [], []);
}
