using FanucTpLsp.Lsp.State;
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
            Detail = $"({kvp.Value.Type}) {kvp.Value.TextDocument.Uri}",
            Documentation = ExtractDocComment(kvp.Value.Program),
            InsertText = $"{Path.GetFileNameWithoutExtension(kvp.Value.TextDocument.Uri).ToUpper()}{ExtractArgsForSnippet(kvp.Value.Program)}",
            InsertTextFormat = InsertTextFormat.Snippet,
            Kind = CompletionItemKind.Function
        }).ToArray();

    // TODO: need to determine the format of the header comment to be able to parse it
    private string ExtractDocComment(TpProgram? program)
        => string.Empty;

    private string ExtractArgsForSnippet(TpProgram? program)
        => string.Empty;
}
