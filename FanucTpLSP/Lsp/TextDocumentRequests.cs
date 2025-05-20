using System.Text.Json.Serialization;

using FanucTpLsp.JsonRPC;

namespace FanucTpLsp.Lsp;

public class TextDocumentPositionParams
{
    [JsonPropertyName("textDocument")]
    public TextDocumentIdentifier TextDocument { get; set; } = new();

    [JsonPropertyName("position")]
    public TextDocumentContentPosition Position { get; set; } = new();
}

public class TextDocumentDidHoverRequest : RequestMessage
{
    [JsonPropertyName("params")]
    public TextDocumentPositionParams Params { get; set; } = new();
}

public class MarkupContent
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "plaintext";

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class HoverResult
{
    [JsonPropertyName("contents")]
    public MarkupContent Contents { get; set; } = new();

    [JsonPropertyName("range")]
    public TextDocumentContentRange Range { get; set; } = new();
}

public class TextDocumentHoverResponse : ResponseMessage
{
    [JsonPropertyName("result")]
    public HoverResult Result { get; set; } = new();
}
