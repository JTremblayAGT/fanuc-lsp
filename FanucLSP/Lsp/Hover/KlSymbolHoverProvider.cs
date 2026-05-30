using FanucLsp.Lsp.State;
using FanucLsp.Lsp.Util;
using KarelParser;
using KarelParser.SymbolTable;

namespace FanucLsp.Lsp.Hover;

internal sealed class KlSymbolHoverProvider : IKlHoverProvider
{
    public HoverResult? GetHoverResult(KarelProgram program, ContentPosition position, TextDocumentItem document, LspServerState state)
        => KarelProgramUtils.GetTokenAt(document.Text, position) switch
        {
            { } token => program.SymTable.GetSymbol(token, new(position.Line + 1, position.Character + 1)) switch
            {
                { } symbol => new HoverResult
                {
                    Contents = BuildHoverInformation(symbol),
                    Range = GetContentRange(position)
                },
                _ => null
            },
            _ => null
        };

    private ContentRange GetContentRange(ContentPosition position)
        => new()
        {
            Start = position,
            End = position
        };

    private MarkupContent BuildHoverInformation(KarelSymbol symbol)
        => new()
        {
            Kind = "markdown",
            Value = $"**{symbol.Name}** ({symbol.Kind})\n"
                  + $"Type: {symbol.Type?.ToString() ?? "none"}\n"
                  + $"@ {symbol.DeclarationPosition}"
        };
}
