using FanucLsp.Lsp.State;
using FanucLSP.Util;
using KarelParser;

namespace FanucLsp.Lsp.Hover;

internal sealed class KlBuiltinHoverProvider : IKlHoverProvider
{
    private Dictionary<string, HoverResult>? _completionItems = null;

    public HoverResult? GetHoverResult(KarelProgram program, ContentPosition position, string content)
        => (_completionItems ??= BuildHoverDict(position))
            ?.GetValueOrDefault(GetTokenAt(content, position));

    private static Dictionary<string, HoverResult> BuildHoverDict(ContentPosition position)
    {
        if (EmbeddedResourceReader.GetKarelBuiltInSnippets() is not { } snippets)
        {
            return [];
        }

        return snippets.ToDictionary(kvp => kvp.Value.Prefix, kvp => new HoverResult
        {
            Range = new ContentRange
            {
                Start = position,
                End = position
            },
            Contents = new MarkupContent
            {
                Kind = "markup",
                Value = string.Join('\n', kvp.Value.Description ?? [])
            }
        });
    }

    private string GetTokenAt(string content, ContentPosition position)
    {
        return string.Empty;
    }
}

