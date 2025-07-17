using System.Text.Json.Serialization;
using FanucLsp.JsonRPC;

namespace FanucLsp.Lsp;

#region Params and Util types

public class TextDocumentIdentifier
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}

public class TextDocumentVersionedIdentifier : TextDocumentIdentifier
{
    [JsonPropertyName("version")]
    public int Version { get; set; } = 0;
}

public class TextDocumentItem
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;

    [JsonPropertyName("languageId")]
    public string LanguageId { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public int Version { get; set; } = 0;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class TextDocumentDidOpenParams
{
    [JsonPropertyName("textDocument")]
    public TextDocumentItem TextDocument { get; set; } = new();
}

public class TextDocumentDidCloseParams
{
    [JsonPropertyName("textDocument")]
    public TextDocumentItem TextDocument { get; set; } = new();
}

public class ContentPosition
{
    [JsonPropertyName("line")]
    public int Line { get; set; } = 0;

    [JsonPropertyName("character")]
    public int Character { get; set; } = 0;
}

public class ContentRange
{
    [JsonPropertyName("start")]
    public ContentPosition Start { get; set; } = new();

    [JsonPropertyName("end")]
    public ContentPosition End { get; set; } = new();

    // TODO: neovim does 0-based lines, need to make sure that VSC*de also does, or figure out how to adjust accordingly
    public static ContentRange WholeFile(string content)
        => new ContentRange
        {
            Start = new ContentPosition
            {
                Line = 0,
                Character = 0
            },
            End = new ContentPosition
            {
                Line = content.Split("\n").Length - 1,
                Character = (content.Split("\n").LastOrDefault()?.Length ?? 1) - 1
            }
        };
}

public class TextDocumentContentChangeEvent
{
    [JsonPropertyName("range")]
    public ContentRange Range { get; set; } = new();

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class TextDocumentDidChangeParams
{
    [JsonPropertyName("textDocument")]
    public TextDocumentVersionedIdentifier TextDocument { get; set; } = new();

    [JsonPropertyName("contentChanges")]
    public TextDocumentContentChangeEvent[] ContentChanges { get; set; } = [];
}

public class TextDocumentDidSaveParams
{
    [JsonPropertyName("textDocument")]
    public TextDocumentIdentifier TextDocument { get; set; } = new();
}

public enum DiagnosticSeverity
{
    Error = 1,
    Warning = 2,
    Information = 3,
    Hint = 4
}

public class Diagnostic
{
    [JsonPropertyName("range")]
    public ContentRange Range { get; set; } = new();

    [JsonPropertyName("severity")]
    public DiagnosticSeverity Severity { get; set; } = DiagnosticSeverity.Error;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class PublishDiagnosticParams
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;

    [JsonPropertyName("diagnostics")]
    public Diagnostic[] Diagnostics { get; set; } = [];
}

#endregion

#region Notifications

public class TextDocumentDidOpenNotification : RequestMessage
{
    [JsonPropertyName("params")]
    public TextDocumentDidOpenParams Params { get; set; } = new();
}

public class TextDocumentDidCloseNotification : RequestMessage
{
    [JsonPropertyName("params")]
    public TextDocumentDidCloseParams Params { get; set; } = new();
}

public class TextDocumentDidChangeNotification : RequestMessage
{
    [JsonPropertyName("params")]
    public TextDocumentDidChangeParams Params { get; set; } = new();
}

public class TextDocumentDidSaveNotification : RequestMessage
{
    [JsonPropertyName("params")]
    public TextDocumentDidSaveParams Params { get; set; } = new();
}

public class PublishDiagnosticsNotification : ResponseMessage
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = "textDocument/publishDiagnostics";

    [JsonPropertyName("params")]
    public PublishDiagnosticParams Params { get; set; } = new();
}

#endregion

