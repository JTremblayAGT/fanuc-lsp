using Sprache;

namespace KarelParser;

/*
 * TODO:
 * Translator directives:
 *
 * The translator directives should probably be parsed (and then removed or ignored)
 * In a first pass to avoid conflicting with the rest of the program for parsing
 */

public sealed record KarelProgram(string Name,
    List<KarelTranslatorDirective> TranslatorDirectives,
    List<KarelDeclaration> Declarations,
    List<KarelRoutine> Routines,
    List<KarelStatement> Statements) : IKarelParser<KarelProgram>
{
    public string HeaderComment = string.Empty;

    public static Parser<KarelProgram> GetParser()
        => from name in KarelCommon.Keyword("PROGRAM").Then(_ => KarelCommon.Identifier).IgnoreComments()
           from translatorDirectives in KarelTranslatorDirective.GetParser().IgnoreComments().XMany()
           from declarations in KarelDeclaration.GetParser().IgnoreComments().XMany()
           from routines in KarelRoutine.GetParser().IgnoreComments().XMany()
           from begin in KarelCommon.Keyword("BEGIN").IgnoreComments()
           from statements in KarelCommon.ParseStatements(["END"]).WithErrorContext("BEGIN")
           from endName in KarelCommon.Keyword("END").Then(_ => KarelCommon.Identifier).IgnoreComments()
           select new KarelProgram(name,
               translatorDirectives.ToList(),
               declarations.ToList(),
               routines.ToList(),
               statements.ToList());

    public static IResult<KarelProgram> ProcessAndParse(string input)
    {
        var lines = input.Split(['\n', '\r']).Select(line => line.Trim()).ToList();

        var headerCommentLines = lines
            .Where(line => !line.StartsWith("%") && !line.StartsWith("PROGRAM"))
            .TakeWhile(line => line.StartsWith("--") || string.IsNullOrWhiteSpace(line))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        return GetParser().WithErrorContext("PROGRAM").TryParse(input) switch
        {
            { WasSuccessful: true } result => Result.Success(result.Value with
            {
                HeaderComment = headerCommentLines.Any() ? headerCommentLines.Aggregate((acc, line) => acc + "\r\n" + line) : string.Empty
            }, result.Remainder),
            { WasSuccessful: false } result => result
        };
    }
}
