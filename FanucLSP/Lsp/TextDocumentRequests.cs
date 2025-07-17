using System.Text.Json.Serialization;
using FanucLsp.JsonRPC;

namespace FanucLsp.Lsp;

public class TextDocumentPositionParams
{
    [JsonPropertyName("textDocument")]
    public TextDocumentIdentifier TextDocument { get; set; } = new();

    [JsonPropertyName("position")]
    public ContentPosition Position { get; set; } = new();
}

public class TextEdit
{
    [JsonPropertyName("range")]
    public ContentRange Range { get; set; } = new();

    [JsonPropertyName("newText")]
    public string NewText { get; set; } = string.Empty;
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
    public ContentRange Range { get; set; } = new();
}

public class TextDocumentHoverResponse : ResponseMessage
{
    [JsonPropertyName("result")]
    public HoverResult? Result { get; set; } = new();
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
    public ContentRange Range { get; set; } = new();
}

public class TextDocumentDefinitionResponse : ResponseMessage
{
    [JsonPropertyName("result")]
    public TextDocumentLocation? Result { get; set; } = new();
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
    public ContentRange Range { get; set; } = new();

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
    public CodeAction[] Result { get; set; } = [];
}

#endregion

#region Completion

public enum TriggerKind
{
    Invoked = 1,
    TriggerCharacter = 2,
    TriggerForIncompleteCompletions = 3
}

public class CompletionContext
{
    [JsonPropertyName("triggerKind")]
    public TriggerKind TriggerKind { get; set; } = TriggerKind.Invoked;

    [JsonPropertyName("triggerCharacter")]
    public string? TriggerCharacter { get; set; } = null;
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

public enum InsertTextFormat
{
    PlainText = 1,
    Snippet = 2
}

public class CompletionItem
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("detail")]
    public string Detail { get; set; } = string.Empty;

    [JsonPropertyName("documentation")]
    public string Documentation { get; set; } = string.Empty;

    [JsonPropertyName("kind")]
    public CompletionItemKind Kind { get; set; } = CompletionItemKind.Text;

    [JsonPropertyName("insertText")]
    public string InsertText { get; set; } = string.Empty;

    [JsonPropertyName("sortText")]
    public string SortText { get; set; } = string.Empty;

    [JsonPropertyName("insertTextFormat")]
    public InsertTextFormat InsertTextFormat { get; set; } = InsertTextFormat.PlainText;
}

public enum CompletionItemKind
{
    Text = 1,
    Method = 2,
    Function = 3,
    Constructor = 4,
    Field = 5,
    Variable = 6,
    Class = 7,
    Interface = 8,
    Module = 9,
    Property = 10,
    Unit = 11,
    Value = 12,
    Enum = 13,
    Keyword = 14,
    Snippet = 15,
    Color = 16,
    File = 17,
    Reference = 18,
    Folder = 19,
    EnumMember = 20,
    Constant = 21,
    Struct = 22,
    Event = 23,
    Operator = 24,
    TypeParameter = 25
}

public class TextDocumentCompletionResponse : ResponseMessage
{
    [JsonPropertyName("result")]
    public CompletionItem[] Result { get; set; } = [];
}

#endregion

#region Formatting

public class TextDocumentFormattingRequest : RequestMessage
{
    [JsonPropertyName("params")]
    public TextDocumentFormattingParams Params { get; set; } = new();
}

public class TextDocumentRangeFormattingRequest : RequestMessage
{
    [JsonPropertyName("params")]
    public TextDocumentRangeFormattingParams Params { get; set; } = new();
}

public class TextDocumentFormattingParams
{
    [JsonPropertyName("textDocument")]
    public TextDocumentIdentifier TextDocument { get; set; } = new();

    [JsonPropertyName("options")]
    public FormattingOptions Options { get; set; } = new();
}

public class TextDocumentRangeFormattingParams
{
    [JsonPropertyName("textDocument")]
    public TextDocumentIdentifier TextDocument { get; set; } = new();

    [JsonPropertyName("range")]
    public ContentRange Range { get; set; } = new();

    [JsonPropertyName("options")]
    public FormattingOptions Options { get; set; } = new();
}

public class FormattingOptions
{
    [JsonPropertyName("tabSize")]
    public int TabSize { get; set; } = 4;

    [JsonPropertyName("insertSpaces")]
    public bool InsertSpaces { get; set; } = true;

    [JsonPropertyName("trimTrailingWhitespace")]
    public bool TrimTrailingSpaces { get; set; } = true;

    [JsonPropertyName("insertFinalNewline")]
    public bool InsertFinalNewline { get; set; } = false;

    [JsonPropertyName("trimFinalNewline")]
    public bool TrimFinalNewline { get; set; } = true;
}

public class TextDocumentFormattingResponse : ResponseMessage
{
    [JsonPropertyName("result")]
    public TextEdit[]? Result { get; set; } = null;
}

#endregion
