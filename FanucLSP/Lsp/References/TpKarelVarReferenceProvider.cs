using FanucLsp.Lsp.State;
using FanucLsp.Lsp.Util;
using ParserUtils;
using TPLangParser;
using KarelParser;
using TPLangParser.TPLang;

namespace FanucLsp.Lsp.References;

internal sealed class TpKarelVarReferenceProvider : ITpReferenceProvider
{
    public TextDocumentLocation[] GetReferences(TpProgram program, ContentPosition position, TextDocumentItem document, ReferenceContext context, LspServerState state)
    {
        throw new NotImplementedException();
    }
}
