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
           from statements in KarelCommon.ParseStatements(["END"])
           from endName in KarelCommon.Keyword("END").Then(_ => KarelCommon.Identifier)
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

        var filteredLines = lines
            .Select(line => line.StartsWith("--") ? string.Empty : line)
            .Select(line => line.Replace("\t", "    "))
            .Select(line => line.Split(" --").First());

        var processedInput = filteredLines.Aggregate((acc, line) => acc + "\r\n" + line);

        return GetParser().TryParse(processedInput) switch
        {
            { WasSuccessful: true } result => Result.Success(result.Value with
            {
                HeaderComment = headerCommentLines.Any() ? headerCommentLines.Aggregate((acc, line) => acc + "\r\n" + line) : string.Empty
            }, result.Remainder),
            { WasSuccessful: false } result => result
        };
    }
}
