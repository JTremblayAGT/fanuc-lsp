using TPLangParser.TPLang;

namespace FanucTpLsp.Lsp.Hover;

internal interface IHoverProvider
{
    public HoverResult? GetHoverResult(TpProgram program, ContentPosition position);
}

