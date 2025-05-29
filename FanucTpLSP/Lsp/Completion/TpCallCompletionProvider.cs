using FanucTpLsp.Lsp.State;
using Sprache;
using TPLangParser.TPLang;

namespace FanucTpLsp.Lsp.Completion;

internal sealed class TpCallCompletionProvider : ICompletionProvider
{
    private const string HeaderCommentDelimiter = "********************************";

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
            Detail = $"Type:  {kvp.Value.Type}\n"
                   + $"Usage: {Path.GetFileNameWithoutExtension(kvp.Value.TextDocument.Uri).ToUpper()}{ExtractArgs(kvp.Value.Program, MakeArgsDetail)}\n"
                   + $"Uri:   {kvp.Value.TextDocument.Uri}",
            Documentation = ExtractDocComment(kvp.Value.Program),
            InsertText = $"{Path.GetFileNameWithoutExtension(kvp.Value.TextDocument.Uri).ToUpper()}{ExtractArgs(kvp.Value.Program, MakeArgsSnippet)}",
            InsertTextFormat = InsertTextFormat.Snippet,
            Kind = CompletionItemKind.Function
        }).ToArray();

    // This is actually already pretty good at extracting arguments
    private static string ExtractDocComment(TpProgram? program)
        => program switch
        {
            not null => program.Main.Instructions
                 .TakeWhile(instr => instr is TpInstructionComment)
                 .Select(instr => (instr as TpInstructionComment)!.Comment)
                 .ToList() switch
            {
                { Count: > 4 } headerComment =>
                    headerComment.RemoveAll(cmt => cmt.StartsWith(HeaderCommentDelimiter)) switch
                    {
                        4 => headerComment.Aggregate((acc, cmt) => acc + "\n" + cmt).Replace("[", "\\[").Replace("]", "\\]"),
                        _ => string.Empty
                    },
                _ => string.Empty
            },
            _ => string.Empty
        };

    private string ExtractArgs(TpProgram? program, Func<int, string> fn)
        => ExtractDocComment(program) switch
        {
            { } comment when !string.IsNullOrWhiteSpace(comment) =>
                comment.Split('\n').Where(cmt => cmt.TrimStart().StartsWith("AR\\[")).ToList() switch
                {
                    { } args => fn(args.Count),
                    _ => string.Empty
                },
            _ => string.Empty
        };

    private string MakeArgsDetail(int count)
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

    private string MakeArgsSnippet(int count)
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
