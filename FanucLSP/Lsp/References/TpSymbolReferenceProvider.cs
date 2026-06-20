using FanucLsp.Lsp.State;
using ParserUtils;
using TPLangParser.TPLang;
using TPLangParser.TPLang.SymbolTable;

namespace FanucLsp.Lsp.References;

internal sealed class TpSymbolReferenceProvider : ITpReferenceProvider
{
    public TextDocumentLocation[] GetReferences(TpProgram program, ContentPosition position, TextDocumentItem document, ReferenceContext context, LspServerState state)
    {
        // Registers, IO ports and system/Karel variables are global to the
        // controller, so a reference search resolves the clicked node to its
        // lookup key once and then collects matching usages from every parsed TP
        // program, not just the current document.
        if (ResolveLookup(program, GetTokenPosition(position)) is not { } lookup)
        {
            return [];
        }

        return state.AllTextDocuments.Values
            .Select(doc => (doc.TextDocument.Uri, Symbol: doc.Program is TppProgram tpp ? lookup(tpp.Program.SymTable) : null))
            .Where(entry => entry.Symbol is not null)
            .SelectMany(entry => entry.Symbol!.Usages
                .Select(usage => new TextDocumentLocation { Uri = entry.Uri, Range = GetContentRange(usage.Position) }))
            .ToArray();
    }

    // Finds the symbol-bearing node under the cursor and turns it into a closure
    // that resolves the same symbol in any program's table. Returns null when the
    // cursor isn't on a register/IO/variable, or on one with no static identity
    // (an indirectly indexed register/port such as R[R[2]]).
    private static Func<TpSymbolTable, TpSymbol?>? ResolveLookup(TpProgram program, TokenPosition position)
    {
        if (program.GetNodeAt<TpGenericRegister>(position) is { } register)
        {
            return TpSymbolTable.TryResolveKey(register, out var kind, out var index)
                ? table => table.GetIndexedSymbol(kind, index)
                : null;
        }

        if (program.GetNodeAt<TpIOPort>(position) is { } port)
        {
            return TpSymbolTable.TryResolveKey(port, out var kind, out var index)
                ? table => table.GetIndexedSymbol(kind, index)
                : null;
        }

        if (program.GetNodeAt<TpFlag>(position) is { } flag)
        {
            return TpSymbolTable.TryResolveKey(flag, out var index)
                ? table => table.GetIndexedSymbol(TpSymbolKind.Flag, index)
                : null;
        }

        if (program.GetNodeAt<TpValueSystemVariable>(position) is { } sysVar)
        {
            return table => table.GetNamedSymbol(TpSymbolKind.SysVar, sysVar.Variable);
        }

        if (program.GetNodeAt<TpValueKarelVariable>(position) is { } karelVar)
        {
            var name = TpSymbolTable.KarelVariableName(karelVar);
            return table => table.GetNamedSymbol(TpSymbolKind.KarelVar, name);
        }

        return null;
    }

    private ContentRange GetContentRange(TokenPosition position)
        => new()
        {
            Start = new ContentPosition { Line = position.Line, Character = position.Column },
            End = new ContentPosition { Line = position.Line, Character = position.Column }
        };

    private TokenPosition GetTokenPosition(ContentPosition position)
        => new(position.Line, position.Character);
}
