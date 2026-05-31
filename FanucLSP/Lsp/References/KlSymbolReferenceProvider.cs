using FanucLsp.Lsp.State;
using FanucLsp.Lsp.Util;
using ParserUtils;
using KarelParser;

namespace FanucLsp.Lsp.References;

internal sealed class KlSymbolReferenceProvider : IKlReferenceProvider
{
    public TextDocumentLocation[] GetReferences(KarelProgram program, ContentPosition position, TextDocumentItem document, ReferenceContext context, LspServerState state)
    {
        var token = KarelProgramUtils.GetTokenAt(document.Text, position);
        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        if (program.SymTable.GetSymbol(token, GetTokenPosition(position)) is not { } symbol)
        {
            return [];
        }

        var refs = program.SymTable.GetSymbolReferences(token, GetTokenPosition(position))
            .Select(pos => new TextDocumentLocation { Uri = document.Uri, Range = GetContentRange(pos) });

        if (context.IncludeDeclaration)
        {
            return refs.Append(new TextDocumentLocation { Uri = document.Uri, Range = GetContentRange(symbol.DeclarationPosition) }).ToArray();
        }

        return refs.ToArray();
    }

    private TokenPosition GetTokenPosition(ContentPosition position)
        => new(position.Line + 1, position.Character + 1);

    private ContentRange GetContentRange(TokenPosition position)
        => new()
        {
            Start = new ContentPosition { Line = position.Line - 1, Character = position.Column - 1 },
            End = new ContentPosition { Line = position.Line - 1, Character = position.Column - 1 }
        };
}
