using FanucLsp.Lsp.State;
using KarelParser;

namespace FanucLsp.Lsp.Definition;

internal interface IKarelDefinitionProvider
{
    public TextDocumentLocation? GetDefinitionLocation(
        KarelProgram program,
        ContentPosition position,
        TextDocumentItem document,
        LspServerState state
    );
}
