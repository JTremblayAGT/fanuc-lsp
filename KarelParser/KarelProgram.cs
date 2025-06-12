using Sprache;

namespace KarelParser;

public sealed record KarelProgram(string Name,
    List<KarelTranslatorDirective> TranslatorDirectives,
    List<KarelDeclaration> Declarations,
    List<KarelRoutine> Routines,
    List<KarelStatement> Statements) : IKarelParser<KarelProgram>
{
    public string HeaderComment = string.Empty;

    public static Parser<KarelProgram> GetParser()
        => from name in KarelCommon.Keyword("PROGRAM").Then(_ => KarelCommon.Identifier)
           from translatorDirectives in KarelTranslatorDirective.GetParser().Many()
           from declarations in KarelDeclaration.GetParser().Many()
           from routines in KarelRoutine.GetParser().Many()
           from begin in KarelCommon.Keyword("BEGIN")
           from statements in KarelStatement.GetParser().Many()
           from endName in KarelCommon.Keyword("END").Then(_ => KarelCommon.Identifier)
           select new KarelProgram(name, translatorDirectives.ToList(), declarations.ToList(), routines.ToList(), statements.ToList());

    public static KarelProgram? ProcessAndParse(string input)
    {
        // TODO: extract header comment at the beginning of the declarations section right after the translator directives
        // TODO: remove ALL comments from input before parsing

        // TODO: split the input into lines
        // TODO: find first line that isn't a translator directive or blank
        // TODO: take lines until blank or not comment -> header comment
        // TODO: filter out all comment lines
        // TODO: remove trailing comments from all other lines
        // TODO: reassemble input and parse

        return KarelProgram.GetParser().TryParse(input).Value;
    }
}
