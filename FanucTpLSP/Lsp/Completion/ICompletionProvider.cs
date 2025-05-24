using System.Text.RegularExpressions;

using TPLangParser.TPLang;

namespace FanucTpLsp.Lsp.Completion;

internal interface ICompletionProvider
{
    public CompletionItem[] GetCompletions(TpProgram program, string lineText, int column);
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
}



