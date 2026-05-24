using FanucLsp.Lsp.State;
using FanucLsp.Lsp.Util;
using ParserUtils;
using KarelParser.SymTable;

namespace FanucLsp.Lsp.Definition;

internal class KarelDefinitionProvider : IKarelDefinitionProvider
{
    public TextDocumentLocation? GetDefinitionLocation(
        KarelSymbolTable symTable,
        ContentPosition position,
        TextDocumentItem document,
        LspServerState state
    )
        => KarelProgramUtils.GetTokenAt(document.Text, position) switch
        {
            { } token => symTable.GetSymbol(token) switch
            {
                { } symbol => new TextDocumentLocation
                {
                    Uri = document.Uri,
                    Range = GetContentRange(symbol.DeclarationPosition)
                },
                _ => null
            },
            _ => null
        };

    private ContentRange GetContentRange(TokenPosition position)
        => new()
        {
            Start = new ContentPosition { Line = position.Line, Character = position.Column },
            End = new ContentPosition { Line = position.Line, Character = position.Column }
        };
}
