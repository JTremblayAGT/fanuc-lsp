using FanucTpLsp.Lsp.State;
using Sprache;
using TPLangParser.TPLang;

namespace FanucTpLsp.Lsp.Completion;

internal sealed class TpCallCompletionProvider : ICompletionProvider
{
    private const string HeaderCommentDelimiter = "******************************** ";

    public CompletionItem[] GetCompletions(TpProgram program, string lineText, int column, LspServerState serverState)
        => CompletionProviderUtils.TokenizeInput(lineText[..column]) switch
        {
            [.., "CALL"] => GetAllProgramNames(serverState),
            _ => []
        };

    private CompletionItem[] GetAllProgramNames(LspServerState serverState)
        => serverState.AllTextDocuments.Select(kvp => new CompletionItem()
        {
            Label = Path.GetFileNameWithoutExtension(kvp.Value.TextDocument.Uri).ToUpper(),
            Detail = $"({kvp.Value.Type}) {kvp.Value.TextDocument.Uri}",
            Documentation = ExtractDocComment(kvp.Value.Program),
            InsertText = $"{Path.GetFileNameWithoutExtension(kvp.Value.TextDocument.Uri).ToUpper()}{ExtractArgsForSnippet(kvp.Value.Program)}",
            InsertTextFormat = InsertTextFormat.Snippet,
            Kind = CompletionItemKind.Function
        }).ToArray();

    private static string ExtractDocComment(TpProgram? program)
        => program switch
        {
            not null => program.Main.Instructions
                 .TakeWhile(instr => instr is TpInstructionComment)
                 .Select(instr => (instr as TpInstructionComment)!.Comment)
                 .ToList() switch
            {
                { Count: > 4 } headerComment =>
                    headerComment.RemoveAll(cmt => cmt.Equals(HeaderCommentDelimiter)) switch
                    {
                        4 => headerComment.Aggregate((acc, cmt) => acc + "\n" + cmt).Replace("[", "\\[").Replace("]", "\\]"),
                        _ => string.Empty
                    },
                _ => string.Empty
            },
            _ => string.Empty
        };

    private string ExtractArgsForSnippet(TpProgram? program)
        => ExtractDocComment(program) switch
        {
            { } comment when !string.IsNullOrWhiteSpace(comment) =>
                comment.Split('\n').Where(cmt => cmt.TrimStart().StartsWith("AR\\[")).ToList() switch
                {
                    { } args => MakeArgsSnippet(args.Count),
                    _ => string.Empty
                },
            _ => string.Empty
        };

    private string MakeArgsSnippet(int count)
    {
        var ret = "(";
        for (var ctr = 1; ctr < count; ++ctr)
        {
            ret += $"${{{ctr}:arg{ctr}}},";

        }
        ret += $"${{{count}:arg{count}}})";
        return ret;
    }
}
