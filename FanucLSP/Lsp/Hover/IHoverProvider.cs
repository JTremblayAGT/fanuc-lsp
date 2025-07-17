using FanucLsp.Lsp.State;
using TPLangParser.TPLang;

namespace FanucLsp.Lsp.Hover;

internal interface IHoverProvider
{
    public HoverResult? GetHoverResult(TpProgram program, ContentPosition position, LspServerState serverState);
}

