using FanucLsp.Lsp.State;
using FanucLsp.Lsp.Util;
using FanucLsp.Util;
using KarelParser;

namespace FanucLsp.Lsp.Hover;

internal sealed class KlBuiltinHoverProvider : IKlHoverProvider
{
    private Dictionary<string, HoverResult>? _completionItems = null;

    public HoverResult? GetHoverResult(
        KarelProgram program,
        ContentPosition position,
        TextDocumentItem document,
        LspServerState state
    ) =>
        (_completionItems ??= BuildHoverDict(position))?.GetValueOrDefault(
            ProgramUtils.GetTokenAt(document.Text, position).ToLower()
        );

    private static Dictionary<string, HoverResult> BuildHoverDict(ContentPosition position)
    {
        if (EmbeddedResourceReader.GetKarelBuiltInSnippets() is not { } snippets)
        {
            return [];
        }

        return snippets.ToDictionary(
            kvp => kvp.Value.Prefix.ToLower(),
            kvp => new HoverResult
            {
                Range = new ContentRange { Start = position, End = position },
                Contents = new MarkupContent
                {
                    Kind = "markup",
                    Value = string.Join('\n', kvp.Value.Description ?? []),
                },
            }
        );
    }
}
