namespace FanucTpLsp.Lsp;

public class LspMethods
{
    // Requests
    public const string Initialize = "initialize";
    public const string Shutdown = "shutdown";

    public const string TextDocumentDidHover = "textDocument/hover";
    public const string TextDocumentDefinition = "textDocument/definition";
    public const string TextDocumentCodeAction = "textDocument/codeAction";
    public const string TextDocumentCompletion = "textDocument/completion";

    // Notifications
    public const string Initialized = "initialized";

    public const string TextDocumentDidOpen = "textDocument/didOpen";
    public const string TextDocumentDidClose = "textDocument/didClose";
    public const string TextDocumentDidChange = "textDocument/didChange";
    public const string TextDocumentDidSave = "textDocument/didSave";
    public const string TextDocumentPublishDiagnostics = "textDocument/publishDiagnostics";
}
