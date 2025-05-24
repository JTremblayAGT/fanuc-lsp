using TPLangParser.TPLang;
using FanucTpLsp.Lsp.Completion;

using Sprache;

namespace FanucTpLsp.Lsp.State;

internal record TextDocumentState(
    TextDocumentItem TextDocument,
    TextDocumentContentPosition LastEditPosition,
    TpProgram? Program
);

internal class LspServerState(string logFilePath)
{
    public bool IsInitialized { get; set; } = false;
    public bool IsShutdown { get; set; } = false;
    public string LastChangedDocumentUri { get; set; } = string.Empty;
    public Dictionary<string, TextDocumentState> OpenedTextDocuments { get; set; } = new();

    private static readonly List<ICompletionProvider> CompletionProviders = [
        new TpLabelCompletion(),
        new TpMotionInstructionCompletion(),
    ];

    public bool OnDocumentOpen(TextDocumentItem document)
    {
        var result = TpProgram.GetParser().TryParse(document.Text);
        if (!result.WasSuccessful)
        {
            // TODO: should actually publish a diagnostic error
        }

        var program = result.WasSuccessful ? result.Value : default(TpProgram);

        OpenedTextDocuments.Add(document.Uri,
            new(document, new(), program));

        return result.WasSuccessful;
    }

    public void UpdateDocumentText(string uri, TextDocumentContentChangeEvent[] changes)
    {
        if (!OpenedTextDocuments.TryGetValue(uri, out var documentState))
        {
            LogMessage($"[TextDocumentDidChange]: Document not opened: {uri}");
            return;
        }

        var text = documentState.TextDocument.Text;
        var lastEditPosition = documentState.LastEditPosition;

        foreach (var change in changes)
        {
            // Apply the change to the document text
            var start = change.Range.Start;
            var end = change.Range.End;
            var newText = change.Text;

            // Convert positions to string indices
            var startIndex = GetOffsetFromPosition(text, start);
            var endIndex = GetOffsetFromPosition(text, end);

            // Apply the change to the text
            if (startIndex >= 0 && endIndex >= 0 && startIndex <= text.Length && endIndex <= text.Length)
            {
                text = text[..startIndex] + newText + text[endIndex..];

                // Update last edit position to end of the inserted text
                lastEditPosition = CalculatePositionAfterEdit(text, start, newText);
            }
            else
            {
                LogMessage($"[TextDocumentDidChange]: Invalid range: {start.Line}:{start.Character} to {end.Line}:{end.Character}");
            }
        }

        // Update the document with the new text
        var document = documentState.TextDocument;
        document.Text = text;

        var result = TpProgram.GetParser().TryParse(document.Text);
        if (!result.WasSuccessful)
        {
            // TODO: should actually publish a diagnostic error
        }

        OpenedTextDocuments[uri] = documentState with
        {
            LastEditPosition = lastEditPosition,
            TextDocument = document,
            Program = result.WasSuccessful ? result.Value : documentState.Program
        };
        LastChangedDocumentUri = uri;
    }

    public CompletionItem[] GetCompletionItems()
    {
        if (!OpenedTextDocuments.TryGetValue(LastChangedDocumentUri, out var documentState))
        {
            LogMessage($"[TextDocumentCompletion]: Document not opened: {LastChangedDocumentUri}");
            return [];
        }

        // Get the document content
        var document = documentState.TextDocument;
        var lastEdit = documentState.LastEditPosition;

        // If we don't have document content, we can't provide completions
        if (string.IsNullOrEmpty(document.Text))
        {
            return [];
        }

        // Split the document into lines
        var lines = document.Text.Split('\n');

        // Make sure the requested position is valid
        if (lastEdit.Line < 1 || lastEdit.Line >= lines.Length)
        {
            return [];
        }

        // Get the current line text
        var currentLine = lines[lastEdit.Line];

        // Make sure the requested character position is valid
        var character = Math.Min(lastEdit.Character, currentLine.Length);

        return CompletionProviders.Aggregate(
                new CompletionItem[] { },
                (accumulator, completionProvider)
                => accumulator.Concat<CompletionItem>(
                        completionProvider.GetCompletions(documentState.Program!, currentLine, character)).ToArray()
        );
    }

    /// <summary>
    /// Converts a line and character position to an offset in the text
    /// </summary>
    private static int GetOffsetFromPosition(string text, TextDocumentContentPosition position)
    {
        // Split the text into lines
        var lines = text.Split('\n');

        // Check if the position is valid
        if (position.Line < 0 || position.Line >= lines.Length)
        {
            return -1;
        }

        // Calculate the offset by summing the lengths of preceding lines plus line breaks
        var offset = 0;
        for (var i = 0; i < position.Line; i++)
        {
            offset += lines[i].Length + 1; // +1 for the newline character
        }

        // Add the character offset (but ensure it doesn't exceed the line length)
        var charOffset = Math.Min(position.Character, lines[position.Line].Length);
        offset += charOffset;

        return offset;
    }

    /// <summary>
    /// Calculates the position after text has been inserted at a given position
    /// </summary>
    private static TextDocumentContentPosition CalculatePositionAfterEdit(string text, TextDocumentContentPosition startPosition, string insertedText)
    {
        // If there are no newlines in the inserted text, we can simply add the length
        if (!insertedText.Contains('\n'))
        {
            return new TextDocumentContentPosition
            {
                Line = startPosition.Line,
                Character = startPosition.Character + insertedText.Length
            };
        }

        // If there are newlines, we need to calculate the new position
        var insertedLines = insertedText.Split('\n');
        return new TextDocumentContentPosition
        {
            Line = startPosition.Line + insertedLines.Length - 1,
            Character = insertedLines[^1].Length + (insertedLines.Length == 1 ? startPosition.Character : 0)
        };
    }

    private void LogMessage(string message)
    {
        try
        {
            File.AppendAllText(logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error logging message: {ex.Message}");
        }
    }
}
