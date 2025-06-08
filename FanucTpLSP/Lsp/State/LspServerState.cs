using TPLangParser.TPLang;
using KarelParser;
using FanucTpLsp.Lsp.Completion;
using FanucTpLsp.Lsp.Definition;
using FanucTpLsp.Lsp.Hover;

using Sprache;

namespace FanucTpLsp.Lsp.State;

public enum DocumentType
{
    Tp,
    Karel
}

public abstract record Program;

public sealed record TppProgram(TpProgram Program) : Program;
public sealed record KlProgram(KarelProgram Program) : Program;

public sealed record TextDocumentState
(
    TextDocumentItem TextDocument,
    ContentPosition LastEditPosition,
    DocumentType Type,
    Program? Program
);

public sealed class LspServerState(string logFilePath)
{
    public bool IsInitialized { get; set; } = false;
    public bool IsShutdown { get; set; } = false;
    public string LastChangedDocumentUri { get; set; } = string.Empty;
    public Dictionary<string, TextDocumentState> OpenedTextDocuments { get; set; } = new();
    public Dictionary<string, TextDocumentState> AllTextDocuments { get; set; } = new();

    private static readonly List<ICompletionProvider> CompletionProviders =
    [
        new TpLabelCompletionProvider(),
        new TpMotionInstructionCompletionProvider(),
        new TpAssignmentCompletionProvider(),
        new TpCallCompletionProvider(),
    ];

    private static readonly List<IDefinitionProvider> DefinitionProviders =
    [
        new TpLabelDefinitionProvider(),
        new TpProgramDefinitionProvider(),
    ];

    private static readonly List<IHoverProvider> HoverProviders =
    [
        new TpLabelHoverProvider(),
        new CallHoverProvider(),
    ];

    public bool Initialize()
    {
        IsInitialized = true;

        // TODO: need to make this a background task (probably with a directory watcher)
        // in order to let the server start faster (maybe later)
        // TODO: We'll also want to index references to stuff in a worker thread
        AllTextDocuments = FindLsAndKlFiles();

        return IsInitialized;
    }

    public IResult<TpProgram> OnTpDocumentOpen(TextDocumentItem document)
    {
        OpenedTextDocuments.Add(document.Uri,
            new(document, new(), DocumentType.Tp, default(Program)));

        return UpdateParsedProgram(document.Uri);
    }

    public Diagnostic[] OnKarelDocumentOpen(TextDocumentItem document)
    {
        OpenedTextDocuments.Add(document.Uri,
            new(document, new(), DocumentType.Karel, default(Program)));

        return [];
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

        OpenedTextDocuments[uri] = documentState with
        {
            LastEditPosition = lastEditPosition,
            TextDocument = document,
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

        if (documentState.Program is not TppProgram prog)
        {
            // TODO: support Karel
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

        var currentLine = lines[lastEdit.Line];
        var character = Math.Min(lastEdit.Character, currentLine.Length);

        return CompletionProviders.Aggregate(
            new CompletionItem[] { }, (accumulator, completionProvider)
                => accumulator.Concat(completionProvider.GetCompletions(prog.Program!, currentLine, character, this))
                    .ToArray()
        );
    }

    public TextDocumentLocation? GetLocation(string uri, ContentPosition position)
    {
        if (OpenedTextDocuments.TryGetValue(uri, out var documentState))
        {
            if (documentState.Program is not TppProgram prog)
            {
                // TODO: support Karel
                return null;
            }
            return DefinitionProviders
                .Select(provider
                    => provider.GetDefinitionLocation(prog.Program!, position, documentState.TextDocument, this))
                .FirstOrDefault(res => res is not null);
        }

        LogMessage($"[TextDocumentDefinition]: Document not opened: {uri}");
        return null;

    }

    public HoverResult? GetHoverResult(string uri, ContentPosition position)
    {
        if (OpenedTextDocuments.TryGetValue(uri, out var documentState))
        {
            if (documentState.Program is not TppProgram prog)
            {
                // TODO: support Karel
                return null;
            }

            return HoverProviders
                .Select(provider => provider.GetHoverResult(prog.Program!, position, this))
                .FirstOrDefault(res => res is not null);
        }

        LogMessage($"[TextDocumentDidHover]: Document not opened: {uri}");
        return null;
    }

    // TODO: need to refactor this to handle both program types
    public IResult<TpProgram> UpdateParsedProgram(string uri)
        => UpdateParsedProgram(OpenedTextDocuments[uri]);

    private IResult<TpProgram> UpdateParsedProgram(TextDocumentState documentState)
    {
        var document = documentState.TextDocument;

        if (documentState.Type == DocumentType.Karel)
        {
            throw new InvalidOperationException($"Karel programs aren't parsed");
        }

        var result = TpProgram.GetParser().TryParse(document.Text);
        OpenedTextDocuments[document.Uri] = documentState with
        {
            Program = result.WasSuccessful ? new TppProgram(result.Value) : documentState.Program
        };
        return result;
    }

    /// <summary>
    /// Converts a line and character position to an offset in the text
    /// </summary>
    private static int GetOffsetFromPosition(string text, ContentPosition position)
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
    private static ContentPosition CalculatePositionAfterEdit(string text, ContentPosition startPosition, string insertedText)
    {
        // If there are no newlines in the inserted text, we can simply add the length
        if (!insertedText.Contains('\n'))
        {
            return new ContentPosition
            {
                Line = startPosition.Line,
                Character = startPosition.Character + insertedText.Length
            };
        }

        // If there are newlines, we need to calculate the new position
        var insertedLines = insertedText.Split('\n');
        return new ContentPosition
        {
            Line = startPosition.Line + insertedLines.Length - 1,
            Character = insertedLines[^1].Length + (insertedLines.Length == 1 ? startPosition.Character : 0)
        };
    }

    private TppProgram? TppValueOr(IResult<TpProgram> result)
        => result.WasSuccessful ? new TppProgram(result.Value) : null;

    private KlProgram? KarelValueOr(IResult<KarelProgram> result)
        => result.WasSuccessful ? new KlProgram(result.Value) : null;

    private Dictionary<string, TextDocumentState> FindLsAndKlFiles()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        LogMessage($"Searching directory for files: {currentDirectory}");
        try
        {
            // Use SearchOption.AllDirectories to recursively search all subdirectories
            var lsFiles = Directory.EnumerateFiles(Path.Combine(currentDirectory, "TPP"), "*.ls", SearchOption.AllDirectories).ToList();
            LogMessage($"Found ${lsFiles.Count} LS files.");

            var klFiles = Directory.EnumerateFiles(Path.Combine(currentDirectory, "KAREL"), "*.kl", SearchOption.AllDirectories).ToList();
            LogMessage($"Found ${klFiles.Count} KL files.");

            Dictionary<string, TextDocumentState> dict = new();
            foreach (var path in lsFiles)
            {
                var text = File.ReadAllText(path);
                dict.Add(path, new(new()
                {
                    Uri = path,
                    LanguageId = "tp",
                    Version = 0,
                    Text = text
                }, new(), DocumentType.Tp, TppValueOr(TpProgram.GetParser().TryParse(text))));
            }
            foreach (var path in klFiles)
            {
                var text = File.ReadAllText(path);
                dict.Add(path, new(new()
                {
                    Uri = path,
                    LanguageId = "karel",
                    Version = 0,
                    Text = text
                }, new(), DocumentType.Karel, KarelValueOr(KarelProgram.GetParser().TryParse(text))));
            }

            return dict;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching for files: {ex.Message}");
            LogMessage($"Error while searching:{ex.GetType()} {ex.Message}");
            return [];
        }
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
