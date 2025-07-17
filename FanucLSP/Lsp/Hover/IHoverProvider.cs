using FanucLsp.Lsp.State;
using KarelParser;
using TPLangParser.TPLang;

namespace FanucLsp.Lsp.Hover;

internal interface IHoverProvider
{
    public HoverResult? GetHoverResult(TpProgram program, ContentPosition position, LspServerState serverState);
}

internal interface IKlHoverProvider
{
    public HoverResult? GetHoverResult(KarelProgram program, ContentPosition position, string content);
}

