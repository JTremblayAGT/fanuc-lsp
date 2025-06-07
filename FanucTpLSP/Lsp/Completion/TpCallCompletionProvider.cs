using FanucTpLsp.Lsp.State;
using Sprache;
using TPLangParser.TPLang;

namespace FanucTpLsp.Lsp.Completion;

internal sealed class TpCallCompletionProvider : ICompletionProvider
{

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
            Documentation = LspUtils.ExtractDocComment(kvp.Value.Program),
            InsertText = $"{Path.GetFileNameWithoutExtension(kvp.Value.TextDocument.Uri).ToUpper()}{ExtractArgs(kvp.Value.Program, MakeArgsSnippet)}",
            InsertTextFormat = InsertTextFormat.Snippet,
            Kind = CompletionItemKind.Function
        }).ToArray();


    private string ExtractArgs(TpProgram? program, Func<int, string> fn)
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
