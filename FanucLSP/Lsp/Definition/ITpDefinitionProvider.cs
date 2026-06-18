using FanucLsp.Lsp.State;
using TPLangParser.TPLang;

namespace FanucLsp.Lsp.Definition;

internal interface ITpDefinitionProvider
{
    public TextDocumentLocation? GetDefinitionLocation(
        TpProgram program,
        ContentPosition position,
        TextDocumentItem document,
        LspServerState state
    );
}
