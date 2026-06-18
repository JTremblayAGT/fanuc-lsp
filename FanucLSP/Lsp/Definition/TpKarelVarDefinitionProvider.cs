using System.Text.RegularExpressions;

using FanucLsp.Lsp.State;
using FanucLsp.Lsp.Util;
using ParserUtils;
using TPLangParser.TPLang;
using TPLangParser.TPLang.SymbolTable;


namespace FanucLsp.Lsp.Definition;

internal sealed partial class TpKarelVarDefinitionProvider : ITpDefinitionProvider
{
    [GeneratedRegex(@"\$\[([a-zA-Z_]+)\]([a-zA-Z_]*(\[[1-9]+\])?\.)*")]
    private static partial Regex Variable();

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

        return klProg.Program.SymTable.GetSymbol(ProgramUtils.GetTokenAt(document.Text, position))
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
            Start = new ContentPosition { Line = position.Line - 1, Character = position.Column - 1 },
            End = new ContentPosition { Line = position.Line - 1, Character = position.Column - 1 }
        };
}
