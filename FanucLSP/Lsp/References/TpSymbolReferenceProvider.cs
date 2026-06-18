using FanucLsp.Lsp.State;
using FanucLsp.Lsp.Util;
using ParserUtils;
using TPLangParser;
using KarelParser;
using TPLangParser.TPLang;
using TPLangParser.TPLang.SymbolTable;

namespace FanucLsp.Lsp.References;

internal sealed class TpSymbolReferenceProvider : ITpReferenceProvider
{
    public TextDocumentLocation[] GetReferences(TpProgram program, ContentPosition position, TextDocumentItem document, ReferenceContext context, LspServerState state)
    {

        return [];
    }

    private TpSymbol? GetSymbolAt(TpProgram program, ContentPosition position, string text)
    {
        if (ProgramUtils.GetKlVariableAt(text, position) is { Length: > 0 } varToken)
        {
            return program.SymTable.GetSymbol(varToken);
        }

        if (ProgramUtils.GetRegisterAt(text, position) is { Length: > 0 } regToken)
        {
            return program.SymTable.GetSymbol(regToken);
        }

        return null;
    }
}
