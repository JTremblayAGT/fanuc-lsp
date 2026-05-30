using TPLangParser.TPLang;
using FanucLsp.Lsp.State;

namespace FanucLsp.Lsp.References;

internal interface ITpReferenceProvider
{
    // TODO:
    public TextDocumentLocation[] GetReferences(TpProgram program, ContentPosition position, TextDocumentItem document, LspServerState state)
        => [];
}
