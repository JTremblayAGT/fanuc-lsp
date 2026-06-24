using System.Text.RegularExpressions;

using FanucLsp.Lsp.State;
using FanucLsp.Lsp.Util;
using ParserUtils;
using TPLangParser.TPLang;
using TPLangParser.TPLang.SymbolTable;


namespace FanucLsp.Lsp.Definition;

internal sealed partial class TpKarelVarDefinitionProvider : ITpDefinitionProvider
{
    public TextDocumentLocation? GetDefinitionLocation(TpProgram program, ContentPosition position, TextDocumentItem document, LspServerState state)
    {
        if (program.SymTable.GetSymbol(ProgramUtils.GetKlVariableAt(document.Text, position))
                is not { Kind: TpSymbolKind.KarelVar } symbol)
        {
            return null;
        }

        var rgxMatch = ProgramUtils.KarelVariable().Match(symbol.Name);
        var programName = rgxMatch.Groups[2].Value;
        var varName = rgxMatch.Groups[3].Value;
        if (state.AllTextDocuments
                .FirstOrDefault(kvp => Path.GetFileNameWithoutExtension(kvp.Key)
                    .Equals(programName, StringComparison.OrdinalIgnoreCase))
                .Value is not {} docState)
        {
            return null;
        }

        if (docState.Program is not KlProgram klProg)
        {
            return null;
        }

        return klProg.Program.SymTable.GetTopLevelSymbol(ProgramUtils.GetTokenAt(document.Text, position))
            ?.DeclarationPosition switch
        {
            { } declPos => new TextDocumentLocation
            {
                Uri = docState.TextDocument.Uri,
                Range = GetContentRange(declPos)
            },
            _ => null
        };
    }

    private ContentRange GetContentRange(TokenPosition position)
        => new()
        {
            Start = new ContentPosition { Line = position.Line, Character = position.Column },
            End = new ContentPosition { Line = position.Line, Character = position.Column }
        };
}
