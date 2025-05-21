using System.Text.Json.Serialization;
using FanucTpLsp.JsonRPC;

namespace FanucTpLsp.Lsp;

public class ServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string? Version { get; set; } = string.Empty;
}

public enum TextDocumentSyncKind
{
    None = 0,
    Full = 1,
    Incremental = 2
}

public class TextDocumentSyncOptions
{
    /// <summary>
    /// Gets or sets the synchronization kind.
    /// </summary>
    [JsonPropertyName("change")]
    public TextDocumentSyncKind Change { get; set; } = TextDocumentSyncKind.Incremental;

    /// <summary>
    /// Gets or sets a value indicating whether to open close notifications.
    /// </summary>
    [JsonPropertyName("openClose")]
    public bool OpenClose { get; set; } = true;
}

public class CompletionOptions
{
    // TODO: leave this empty for now, see if we need to add anything later
}

public class ServerCapabilities
{
    [JsonPropertyName("textDocumentSync")]
    public TextDocumentSyncOptions TextDocumentSync { get; set; } = new();

    [JsonPropertyName("hoverProvider")]
    public bool HoverProvider { get; set; } = true;

    [JsonPropertyName("definitionProvider")]
    public bool DefinitionProvider { get; set; } = true;

    [JsonPropertyName("codeActionProvider")]
    public bool CodeActionProvider { get; set; } = true;

    [JsonPropertyName("completionProvider")]
    public CompletionOptions CompletionProvider { get; set; } = new();
}

public class InitializeResult
{
    /// <summary>
    /// Gets or sets the capabilities of the server.
    /// </summary>
    [JsonPropertyName("capabilities")]
    public ServerCapabilities Capabilities { get; set; } = new();

    /// <summary>
    /// Gets or sets the information of the server.
    /// </summary>
    [JsonPropertyName("serverInfo")]
    public ServerInfo ServerInfo { get; set; } = new();
}

public class InitializeResponse : ResponseMessage
{
    /// <summary>
    /// Gets or sets the result of the initialize request.
    /// </summary>
    [JsonPropertyName("result")]
    public InitializeResult Result { get; set; } = new();
}

