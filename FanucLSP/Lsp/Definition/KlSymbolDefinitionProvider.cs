using FanucLsp.Lsp.State;
using FanucLsp.Lsp.Util;
using ParserUtils;
using KarelParser;

namespace FanucLsp.Lsp.Definition;

internal class KlSymbolDefinitionProvider : IKlDefinitionProvider
{
    public TextDocumentLocation? GetDefinitionLocation(
        KarelProgram program,
        ContentPosition position,
        TextDocumentItem document,
        LspServerState state
    )
        => ProgramUtils.GetTokenAt(document.Text, position) switch
        {
            { } token => program.SymTable.GetSymbol(token, GetTokenPosition(position)) switch
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

    private TokenPosition GetTokenPosition(ContentPosition position)
        => new(position.Line + 1, position.Character + 1);

    private ContentRange GetContentRange(TokenPosition position)
        => new()
        {
            Start = new ContentPosition { Line = position.Line - 1, Character = position.Column - 1 },
            End = new ContentPosition { Line = position.Line - 1, Character = position.Column - 1 }
        };
}
