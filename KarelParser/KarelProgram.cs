using KarelParser.SymbolTable;
using Sprache;

using ParserUtils;

namespace KarelParser;

public sealed record KarelProgram(
    string Name,
    List<KarelTranslatorDirective> TranslatorDirectives,
    List<KarelDeclaration> Declarations,
    List<KarelRoutine> Routines,
    List<KarelStatement> Statements
) : WithPosition, IKarelParser<KarelProgram>
{
    public KarelSymbolTable SymTable { get; init; } = new();

    public string Uri { get; init; } = string.Empty;
    public string LocalPath { get; init; } = string.Empty;
    public string HeaderComment { get; init; } = string.Empty;

    private static readonly Parser<KarelProgram> InternalParser =
        from name in KarelCommon
            .Keyword("PROGRAM")
            .Then(_ => KarelCommon.Identifier)
            .IgnoreComments()
        from translatorDirectives in KarelTranslatorDirective.GetParser().IgnoreComments().XMany()
        from declarations in KarelDeclaration.GetParser().IgnoreComments().XMany()
        from translatorDirectives2 in KarelTranslatorDirective.GetParser().IgnoreComments().XMany()
        from routines in KarelRoutine.GetParser().IgnoreComments().XMany()
        from translatorDirectives3 in KarelTranslatorDirective.GetParser().IgnoreComments().XMany()
        from begin in KarelCommon.Keyword("BEGIN").IgnoreComments()
        from statements in KarelCommon.ParseStatements(["END"]).WithErrorContext("BEGIN")
        from endName in KarelCommon
            .Keyword("END")
            .Then(_ => KarelCommon.Identifier)
            .IgnoreComments()
        select new KarelProgram(
            name,
            translatorDirectives.Concat(translatorDirectives2).Concat(translatorDirectives3).ToList(),
            declarations.ToList(),
            routines.ToList(),
            statements.ToList()
        );

    private static string ExpandIncludeDirectives(string source, string directory)
        => string.Join("\n", source.Split(new string[] {"\r\n", "\r", "\n"}, StringSplitOptions.None).Select(ln => ln.Trim().Split(['\t', ' ']) switch
        {
            ["%INCLUDE" or "%include", var file] => $"%INCLUDE {Path.Join(directory, file)}.kl",
            _ => ln
        }));

    public static Parser<KarelProgram> GetParser() => InternalParser.WithPos();

    public static IResult<KarelProgram> ProcessAndParse(string uri)
    {
        var path = new Uri(uri).LocalPath;
        if (Path.GetDirectoryName(path) is not {} directory)
        {
            return Result.Failure<KarelProgram>(null, $"Could not extract directory from {path}", []);
        }

        var input = File.ReadAllText(path);
        var lines = input.Split(['\n', '\r']).Select(line => line.Trim()).ToList();
        var headerCommentLines = lines
            .Where(line => !line.StartsWith("%") && !line.StartsWith("PROGRAM"))
            .TakeWhile(line => line.StartsWith("--") || string.IsNullOrWhiteSpace(line))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        return GetParser().WithErrorContext("PROGRAM").TryParse(ExpandIncludeDirectives(input, directory)) switch
        {
            { WasSuccessful: true } result => Result.Success(
                result.Value with
                {
                    Uri = uri,
                    LocalPath = path,
                    HeaderComment = headerCommentLines.Any()
                        ? headerCommentLines.Aggregate((acc, line) => acc + "\r\n" + line)
                        : string.Empty,
                    SymTable = KarelSymbolTableBuilder.Build(result.Value)
                },
                result.Remainder
            ),
            { WasSuccessful: false } result => result,
        };
    }
}
