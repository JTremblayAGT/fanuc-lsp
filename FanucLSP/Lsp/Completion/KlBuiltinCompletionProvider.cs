using FanucLsp.Lsp.State;
using FanucLSP.Util;
using KarelParser;

namespace FanucLsp.Lsp.Completion;

internal sealed class KlBuiltinCompletionProvider : IKlCompletionProvider
{
    private CompletionItem[]? _completionItems = null;

    public CompletionItem[] GetCompletions(KarelProgram program, string lineText, int column, LspServerState serverState)
        => _completionItems ??= BuildCompletionList();

    private static CompletionItem[] BuildCompletionList()
    {
        if (EmbeddedResourceReader.GetKarelBuiltInSnippets() is not { } snippets)
        {
            return [];
        }

        return snippets.Select(kvp => new CompletionItem
        {
            Label = kvp.Value.Prefix,
            Detail = kvp.Key,
            Documentation = string.Join('\n', kvp.Value.Description ?? []),
            Kind = CompletionItemKind.Function,
            InsertText = kvp.Value.Body.FirstOrDefault() ?? kvp.Value.Prefix,
            InsertTextFormat = InsertTextFormat.Snippet
        }).ToArray();
    }
}
