using FanucLsp.Lsp.State;

using KarelParser;
using KarelParser.SymbolTable;

namespace FanucLsp.Lsp.References;

internal interface IKlReferenceProvider
{
    // TODO:
    public TextDocumentLocation[] GetReferences(KarelProgram program, ContentPosition position, TextDocumentItem document, LspServerState state)
        => [];
}
