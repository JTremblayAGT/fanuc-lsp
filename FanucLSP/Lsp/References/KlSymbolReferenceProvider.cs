using FanucLsp.Lsp.State;
using FanucLsp.Lsp.Util;
using ParserUtils;
using KarelParser;

namespace FanucLsp.Lsp.References;

internal sealed class KlSymbolReferenceProvider : IKlReferenceProvider
{
    public TextDocumentLocation[] GetReferences(
        KarelProgram program,
        ContentPosition position,
        TextDocumentItem document,
        ReferenceContext context,
        LspServerState state
    )
    {
        var token = ProgramUtils.GetTokenAt(document.Text, position);
        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        if (program.SymTable.GetSymbol(token, GetTokenPosition(position)) is not { } symbol)
        {
            return [];
        }

        var refs = program.SymTable.GetSymbolReferences(token, GetTokenPosition(position))
            .Select(pos => new TextDocumentLocation { Uri = pos.ProgramUri.ToString(), Range = GetContentRange(pos.Position) });

        if (context.IncludeDeclaration)
        {
            return refs.Append(new TextDocumentLocation 
            {
                Uri = symbol.DeclarationPosition.ProgramUri.ToString(),
                Range = GetContentRange(symbol.DeclarationPosition.Position) 
            }).ToArray();
        }

        return refs.ToArray();
    }

    private TokenPosition GetTokenPosition(ContentPosition position)
        => new(position.Line, position.Character);

    private ContentRange GetContentRange(TokenPosition position)
        => new()
        {
            Start = new ContentPosition { Line = position.Line, Character = position.Column },
            End = new ContentPosition { Line = position.Line, Character = position.Column }
        };
}
