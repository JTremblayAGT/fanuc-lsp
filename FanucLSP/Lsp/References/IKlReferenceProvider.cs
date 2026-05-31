using FanucLsp.Lsp.State;

using KarelParser;

namespace FanucLsp.Lsp.References;

internal interface IKlReferenceProvider
{
    public TextDocumentLocation[] GetReferences(KarelProgram program, ContentPosition position, TextDocumentItem document, ReferenceContext context, LspServerState state);
}
