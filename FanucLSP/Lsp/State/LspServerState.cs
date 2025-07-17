using TPLangParser.TPLang;
using KarelParser;
using Sprache;
using System.Collections.Concurrent;
using FanucLsp.Lsp.Completion;
using FanucLsp.Lsp.Definition;
using FanucLsp.Lsp.Hover;

namespace FanucLsp.Lsp.State;

public enum DocumentType
{
    Tp,
    Karel
}

public abstract record RobotProgram;

public sealed record TppProgram(TpProgram Program) : RobotProgram;
public sealed record KlProgram(KarelProgram Program) : RobotProgram;

public sealed record TextDocumentState
(
    TextDocumentItem TextDocument,
    ContentPosition LastEditPosition,
    DocumentType Type,
    RobotProgram? Program
);

public sealed class LspServerState(string logFilePath)
{
    public bool IsInitialized { get; set; } = false;
    public bool IsShutdown { get; set; } = false;
    public string LastChangedDocumentUri { get; set; } = string.Empty;
    public ConcurrentDictionary<string, TextDocumentState> OpenedTextDocuments { get; set; } = new();
    public ConcurrentDictionary<string, TextDocumentState> AllTextDocuments { get; set; } = new();

    private static readonly List<ICompletionProvider> TpCompletionProviders =
    [
        new TpLabelCompletionProvider(),
        new TpMotionInstructionCompletionProvider(),
        new TpAssignmentCompletionProvider(),
        new TpCallCompletionProvider(),
        new KlVariableCompletionProvider(),
    ];

    private static readonly List<IKlCompletionProvider> KlCompletionProviders =
    [
        new KlBuiltinCompletionProvider()
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

    private static readonly List<IKlHoverProvider> klHoverProviders =
    [
        new KlBuiltinHoverProvider()
    ];

    public bool Initialize()
    {
        IsInitialized = true;

        // TODO: need to make this a background task (probably with a directory watcher)
        // in order to let the server start faster (maybe later)
        // TODO: We'll also want to index references to stuff in a worker thread
        Task.Run(() => FindLsFiles());
        Task.Run(() => FindKlFiles());
        Task.Run(() => BuildSysVarIndex());
        Task.Run(() => BuildKlBuiltinIndex());

        return IsInitialized;
    }

    public async Task<IResult<TpProgram>> OnTpDocumentOpen(TextDocumentItem document)
    {
        OpenedTextDocuments.TryAdd(document.Uri,
            new(document, new(), DocumentType.Tp, default(RobotProgram)));

        return await UpdateParsedProgram(document.Uri);
    }

    public async Task<IResult<KarelProgram>> OnKarelDocumentOpen(TextDocumentItem document)
    {
        OpenedTextDocuments.TryAdd(document.Uri,
            new(document, new(), DocumentType.Karel, default(RobotProgram)));

        return await UpdateParsedKlProgram(document.Uri);
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

        return documentState.Program switch
        {
            TppProgram tpp => GetTpCompletionItems(tpp.Program, currentLine, character),
            KlProgram kl => GetKlCompletionItems(kl.Program, currentLine, character),
            _ => []
        };
    }

    private CompletionItem[] GetTpCompletionItems(TpProgram program, string currentLine, int character)
        => TpCompletionProviders.Aggregate(
            new CompletionItem[] { }, (accumulator, completionProvider)
                => accumulator.Concat(completionProvider.GetCompletions(program, currentLine, character, this))
                    .ToArray()
        );

    private CompletionItem[] GetKlCompletionItems(KarelProgram program, string currentLine, int character)
        => KlCompletionProviders.Aggregate(
            new CompletionItem[] { }, (accumulator, completionProvider)
                => accumulator.Concat(completionProvider.GetCompletions(program, currentLine, character, this))
                    .ToArray()
        );

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
            return documentState.Program switch
            {
                TppProgram tpProg => HoverProviders
                    .Select(provider => provider.GetHoverResult(tpProg.Program!, position, this))
                    .FirstOrDefault(res => res is not null),
                KlProgram klProg => klHoverProviders
                    .Select(provider => provider.GetHoverResult(klProg.Program!, position, documentState.TextDocument.Text))
                    .FirstOrDefault(res => res is not null),
                _ => null
            };
        }

        LogMessage($"[TextDocumentDidHover]: Document not opened: {uri}");
        return null;
    }

    // TODO: need to refactor this to handle both program types
    public async Task<IResult<TpProgram>> UpdateParsedProgram(string uri)
        => await UpdateParsedProgram(OpenedTextDocuments[uri]);

    public async Task<IResult<KarelProgram>> UpdateParsedKlProgram(string uri)
        => await UpdateParsedKlProgram(OpenedTextDocuments[uri]);

    private async Task<IResult<TpProgram>> UpdateParsedProgram(TextDocumentState documentState)
    {
        var document = documentState.TextDocument;

        switch (documentState.Type)
        {
            case DocumentType.Tp:
                var result = await Task.Run(() => TpProgram.GetParser().TryParse(document.Text));
                OpenedTextDocuments[document.Uri] = documentState with
                {
                    Program = result.WasSuccessful ? new TppProgram(result.Value) : documentState.Program
                };
                return result;
            default:
                throw new InvalidOperationException($"Not a TPP program");
        }
    }

    private async Task<IResult<KarelProgram>> UpdateParsedKlProgram(TextDocumentState documentState)
    {
        var document = documentState.TextDocument;

        switch (documentState.Type)
        {
            case DocumentType.Karel:
                var result = await Task.Run(() => KarelProgram.ProcessAndParse(document.Text));
                OpenedTextDocuments[document.Uri] = documentState with
                {
                    Program = result.WasSuccessful ? new KlProgram(result.Value) : documentState.Program
                };
                return result;
            default:
                throw new InvalidOperationException($"Not a Karel program");
        }
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

    private static TppProgram? TppValueOr(IResult<TpProgram> result)
        => result.WasSuccessful ? new TppProgram(result.Value) : null;

    private static KlProgram? KarelValueOr(IResult<KarelProgram> result)
        => result.WasSuccessful ? new KlProgram(result.Value) : null;

    private void FindLsFiles()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        LogMessage($"Searching directory for files: {currentDirectory}");
        try
        {
            // Use SearchOption.AllDirectories to recursively search all subdirectories
            var lsFiles = Directory.EnumerateFiles(Path.Combine(currentDirectory, "TPP"), "*.ls", SearchOption.AllDirectories).ToList();
            LogMessage($"Found ${lsFiles.Count} LS files.");

            Parallel.ForEach(lsFiles, path =>
            {
                var text = File.ReadAllText(path);
                AllTextDocuments.TryAdd(path, new(new()
                {
                    Uri = path,
                    LanguageId = "tp",
                    Version = 0,
                    Text = text
                }, new(), DocumentType.Tp, TppValueOr(TpProgram.GetParser().TryParse(text))));
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching for files: {ex.Message}");
            LogMessage($"Error while searching:{ex.GetType()} {ex.Message}");
        }
    }

    private void FindKlFiles()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        LogMessage($"Searching directory for files: {currentDirectory}");
        try
        {
            var klFiles = Directory.EnumerateFiles(Path.Combine(currentDirectory, "KAREL"), "*.kl", SearchOption.AllDirectories).ToList();
            LogMessage($"Found ${klFiles.Count} KL files.");

            Parallel.ForEach(klFiles, path =>
            {
                var text = File.ReadAllText(path);
                AllTextDocuments.TryAdd(path, new(new()
                {
                    Uri = path,
                    LanguageId = "karel",
                    Version = 0,
                    Text = text
                }, new(), DocumentType.Karel, KarelValueOr(KarelProgram.GetParser().TryParse(text))));
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching for files: {ex.Message}");
            LogMessage($"Error while searching:{ex.GetType()} {ex.Message}");
        }
    }

    private void BuildSysVarIndex()
    {
        // TODO: Parse VA files in resources & build tree
    }

    private void BuildKlBuiltinIndex()
    {
        // TODO: Parse karelbuiltin.code-snippets file into dict
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
