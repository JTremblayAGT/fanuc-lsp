
namespace FanucTpLsp.Lsp.State;

internal class LspServerState
{
    public bool IsInitialized { get; set; } = false;
    public bool IsShutdown { get; set; } = false;
    public Dictionary<string, TextDocumentItem> OpenedTextDocuments { get; set; } = new();
}
