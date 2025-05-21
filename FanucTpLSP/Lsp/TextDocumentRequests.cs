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

#region Hover

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

#endregion

#region Definition

public class TextDocumentDefinitionRequest : RequestMessage
{
    [JsonPropertyName("params")]
    public TextDocumentPositionParams Params { get; set; } = new();
}

public class TextDocumentLocation
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;

    [JsonPropertyName("range")]
    public TextDocumentContentRange Range { get; set; } = new();
}

public class TextDocumentDefinitionResponse : ResponseMessage
{
    [JsonPropertyName("result")]
    public TextDocumentLocation Result { get; set; } = new();
}

#endregion

#region Code Actions

public class CodeActionEdit
{
    // TODO: Implement CodeActionEdit properties
}

public class CodeActionCommand
{
    // TODO: Implement CodeActionCommand properties
}

public class CodeAction
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("edit")]
    public CodeActionEdit? Edit { get; set; } = null;

    [JsonPropertyName("command")]
    public CodeActionCommand? Command { get; set; } = null;
}

public class CodeActionContext
{
    // TODO: Implement CodeActionContext properties
}

public class TextDocumentCodeActionParams
{
    [JsonPropertyName("textDocument")]
    public TextDocumentIdentifier TextDocument { get; set; } = new();

    [JsonPropertyName("range")]
    public TextDocumentContentRange Range { get; set; } = new();

    [JsonPropertyName("context")]
    public CodeActionContext Context { get; set; } = new();
}

public class TextDocumentCodeActionRequest : RequestMessage
{
    [JsonPropertyName("params")]
    public TextDocumentPositionParams Params { get; set; } = new();
}

public class TextDocumentCodeActionResponse : ResponseMessage
{
    [JsonPropertyName("result")]
    public CodeAction[] Result { get; set; } = Array.Empty<CodeAction>();
}

#endregion

#region Completion

public class CompletionContext
{
    // TODO: Implement CompletionContext properties
}

public class TextDocumentCompletionParams
{
    [JsonPropertyName("context")]
    public CompletionContext? Context { get; set; } = null;
}

public class TextDocumentCompletionRequest : RequestMessage
{
    [JsonPropertyName("params")]
    public TextDocumentCompletionParams Params { get; set; } = new();
}

public class TextDocumentCompletionItem
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("detail")]
    public string Detail { get; set; } = string.Empty;

    [JsonPropertyName("documentation")]
    public string Documentation { get; set; } = string.Empty;
}

public class TextDocumentCompletionResponse : ResponseMessage
{
    [JsonPropertyName("result")]
    public TextDocumentCompletionItem[] Result { get; set; } = Array.Empty<TextDocumentCompletionItem>();
}

#endregion
