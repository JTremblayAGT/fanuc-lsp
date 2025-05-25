using TPLangParser.TPLang;

namespace FanucTpLsp.Lsp.Hover;

internal class TpProgramHoverProvider : IHoverProvider
{
    public HoverResult? GetHoverResult(TpProgram program, ContentPosition position)
    {
        // TODO: ensure the token at [position] is a program name (in a CALL instruction)
        // Search all documents for a file with that name and return its path and type (tp or karel)
        // fun idea: could parse comment at the beginning (with specific format) to find info on arg registers
        throw new NotImplementedException();
    }
}
