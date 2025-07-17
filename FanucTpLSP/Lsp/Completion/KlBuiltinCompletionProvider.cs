using System.Text.Json;
using FanucTpLsp.Lsp.State;
using KarelParser;

namespace FanucTpLsp.Lsp.Completion;

internal sealed class KlBuiltinCompletionProvider : IKlCompletionProvider
{
    private CompletionItem[]? completionItems = null;

    public CompletionItem[] GetCompletions(KarelProgram program, string lineText, int column, LspServerState serverState)
        => completionItems ??= BuildCompletionList();

    private CompletionItem[] BuildCompletionList()
    {

        var snippets = JsonSerializer.Deserialize<Dictionary<string, CodeSnippet>>
            (File.ReadAllText(@"C:\Users\justin.tremblay\Projects\fanuctp-lsp\FanucTpLSP\Resources\karelbuiltin.code-snippets"));

        if (snippets is null)
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
