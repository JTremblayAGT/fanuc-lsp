using FanucLsp.Lsp.State;
using KarelParser.SymTable;

namespace FanucLsp.Lsp.Definition;

internal interface IKarelDefinitionProvider
{
    public TextDocumentLocation? GetDefinitionLocation(
        KarelSymbolTable symTable,
        ContentPosition position,
        TextDocumentItem document,
        LspServerState state
    );
}
