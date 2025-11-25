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
        var lines = content.Split('\n');
        if (position.Line < 0 || position.Line >= lines.Length)
        {
            return string.Empty;
        }

        var line = lines[position.Line];
        if (position.Character < 0 || position.Character >= line.Length)
        {
            return string.Empty;
        }

        // Find the start of the identifier
        var start = position.Character;
        while (start > 0 && IsIdentifierChar(line[start - 1]))
        {
            start--;
        }

        // Find the end of the identifier
        var end = position.Character;
        while (end < line.Length && IsIdentifierChar(line[end]))
        {
            end++;
        }

        // Extract the identifier
        if (start < end && IsIdentifierStart(line[start]))
        {
            return line.Substring(start, end - start);
        }

        return string.Empty;
    }

    private static bool IsIdentifierStart(char c)
        => char.IsLetter(c) || c == '_';

    private static bool IsIdentifierChar(char c)
        => char.IsLetterOrDigit(c) || c == '_';
}

