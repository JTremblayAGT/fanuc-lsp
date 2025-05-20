using System.Text.Json.Serialization;

using FanucTpLsp.JsonRPC;

namespace FanucTpLsp.Lsp;

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

public class TextDocumentContentPosition
{
    [JsonPropertyName("line")]
    public int Line { get; set; } = 0;

    [JsonPropertyName("character")]
    public int Character { get; set; } = 0;
}

public class TextDocumentContentRange
{
    [JsonPropertyName("start")]
    public TextDocumentContentPosition Start { get; set; } = new();

    [JsonPropertyName("end")]
    public TextDocumentContentPosition End { get; set; } = new();
}

public class TextDocumentContentChangeEvent
{
    [JsonPropertyName("range")]
    public TextDocumentContentRange Range { get; set; } = new();

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

#endregion

