using FanucLsp.Lsp.State;
using TPLangParser.TPLang;

namespace FanucLsp.Lsp.Definition;

internal interface IDefinitionProvider
{
    public TextDocumentLocation? GetDefinitionLocation(TpProgram program, ContentPosition position, TextDocumentItem document, LspServerState state);
}
