using FanucLsp.Lsp.State;
using FanucLsp.Lsp.Util;
using ParserUtils;
using KarelParser;

namespace FanucLsp.Lsp.References;

internal sealed class KlExternalReferenceProvider : IKlReferenceProvider
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

        // Don't check in routine scopes, only top-level variables can be referenced outside the program
        if (program.SymTable.GetSymbol(token) is not { } symbol)
        {
            return [];
        }

        var fullName = $"$[{program.Name}]{symbol.FullName}";
        return state.AllTextDocuments.Values
            .Where(doc => doc.Program is TppProgram)
            .SelectMany(doc => ((TppProgram)doc.Program!).Program.SymTable.GetSymbolReferences(fullName)
                    .Select(refpos => new TextDocumentLocation { Uri = doc.TextDocument.Uri, Range = GetContentRange(refpos) }))
            .ToArray();
    }

    private ContentRange GetContentRange(TokenPosition position)
        => new()
        {
            Start = new ContentPosition { Line = position.Line - 1, Character = position.Column - 1 },
            End = new ContentPosition { Line = position.Line - 1, Character = position.Column - 1 }
        };
}
