using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using FanucTpLsp.Lsp.State;
using KarelParser;
using TPLangParser.TPLang;

namespace FanucTpLsp.Lsp.Completion;

internal interface ICompletionProvider
{
    public CompletionItem[] GetCompletions(TpProgram program, string lineText, int column, LspServerState serverState);
}

internal interface IKlCompletionProvider
{
    public CompletionItem[] GetCompletions(KarelProgram program, string lineText, int column, LspServerState serverState);
}

internal struct CodeSnippet
{
    [JsonPropertyName("prefix")]
    public string Prefix = string.Empty;

    [JsonPropertyName("body")]
    public List<string> Body = [];

    [JsonPropertyName("description")]
    public List<string>? Description = [];

    public CodeSnippet()
    {
    }
}

internal class CompletionProviderUtils
{
    public static List<string> TokenizeInput(string input)
    {
        // First, remove line number if present (format: "123: ")
        var lineWithoutNumber = RemoveLineNumber(input);

        // Simple tokenization - split by whitespace but preserve quoted strings
        var tokens = new List<string>();
        const string pattern = """
                                   [^\s"]+|"[^"]*\"
                                   """;
        var matches = Regex.Matches(lineWithoutNumber, pattern);

        foreach (Match match in matches)
        {
            tokens.Add(match.Value);
        }

        return tokens;
    }

    public static string RemoveLineNumber(string input)
    {
        // Match pattern like "123: " at the beginning of the line
        const string lineNumberPattern = @"^\s*\d+\s*:";
        var match = Regex.Match(input, lineNumberPattern);

        return match.Success ?
            // Strip off the line number and the colon
            input[match.Value.Length..].TrimStart() : input;
    }

    public static CompletionItem[] GetKarelProgramNames(LspServerState serverState)
        => serverState.AllTextDocuments
            .Where(kvp => kvp.Value.Program is KlProgram)
            .Select(kvp => new CompletionItem
            {
                Label = Path.GetFileNameWithoutExtension(kvp.Value.TextDocument.Uri).ToUpper(),
                Detail = string.Empty,
                Documentation = string.Empty,
                InsertText = Path.GetFileNameWithoutExtension(kvp.Value.TextDocument.Uri).ToUpper(),
                InsertTextFormat = InsertTextFormat.PlainText,
                Kind = CompletionItemKind.Class
            }).ToArray();

    public static CompletionItem[] GetAllProgramNames(LspServerState serverState)
        => serverState.AllTextDocuments.Select(kvp => new CompletionItem()
        {
            Label = Path.GetFileNameWithoutExtension(kvp.Value.TextDocument.Uri).ToUpper(),
            Detail = $"Type:  {kvp.Value.Type}\n"
                   + $"Usage: {Path.GetFileNameWithoutExtension(kvp.Value.TextDocument.Uri).ToUpper()}{ExtractArgs(kvp.Value.Program, MakeArgsDetail)}\n"
                   + $"Uri:   {kvp.Value.TextDocument.Uri}",
            Documentation = LspUtils.ExtractDocComment(kvp.Value.Program),
            InsertText = $"{Path.GetFileNameWithoutExtension(kvp.Value.TextDocument.Uri).ToUpper()}{ExtractArgs(kvp.Value.Program, MakeArgsSnippet)}",
            InsertTextFormat = InsertTextFormat.Snippet,
            Kind = CompletionItemKind.Function
        }).ToArray();

    public static string ExtractArgs(RobotProgram? program, Func<int, string> fn)
        => LspUtils.ExtractDocComment(program) switch
        {
            { } comment when !string.IsNullOrWhiteSpace(comment) =>
                comment.Split('\n').Where(cmt => cmt.TrimStart().StartsWith("AR\\[")).ToList() switch
                {
                    { } args => fn(args.Count),
                    _ => string.Empty
                },
            _ => string.Empty
        };

    public static string MakeArgsDetail(int count)
    {
        if (count == 0)
        {
            return string.Empty;
        }

        var ret = "(";
        for (var ctr = 1; ctr < count; ++ctr)
        {
            ret += $"arg{ctr},";
        }
        ret += $"arg{count})";
        return ret;
    }

    public static string MakeArgsSnippet(int count)
    {
        if (count == 0)
        {
            return string.Empty;
        }

        var ret = "(";
        for (var ctr = 1; ctr < count; ++ctr)
        {
            ret += $"${{{ctr}:arg{ctr}}},";
        }
        ret += $"${{{count}:arg{count}}})";
        return ret;
    }

}



