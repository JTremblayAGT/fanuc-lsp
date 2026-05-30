using ParserUtils;

namespace KarelParser.SymbolTable;

public enum KarelSymbolKind
{
    Variable,
    Routine,
    Type,
    StructField,
    Constant,
}

public class KarelSymbol
{
    public string Name { get; }
    public KarelSymbolKind Kind { get; }
    public TokenPosition DeclarationPosition { get; }
    public List<TokenPosition> UsagePositions { get; }
    public KarelUserType? Type { get; }

    public KarelSymbol(string name, KarelSymbolKind kind, KarelUserType? type, TokenPosition declarationPosition)
    {
        Name = name;
        Kind = kind;
        Type = type;
        DeclarationPosition = declarationPosition;
        UsagePositions = new();
    }
}

// KarelSymbolTable needs to be a recursive structure to properly represent lexical scoping
public class KarelSymbolTable
{
    // There should never be more than one level of children realistically
    List<KarelSymbolTable> Routines { get; set;} = [];

    public TokenPosition ScopeStart { get; set; } = new(0,0);
    public TokenPosition ScopeEnd { get; set; } = new(0,0);

    private readonly Dictionary<string, KarelSymbol> _symbols = new();

    public KarelSymbolTable CreateRoutine(TokenPosition start, TokenPosition end)
    {
        if (!(IsPositionInScope(start) && (IsPositionInScope(end))))
        {
            throw new InvalidOperationException("Nested scope isn't in parent scope");
        }
        var childTbl = new KarelSymbolTable
        {
            ScopeStart = start,
            ScopeEnd = end,
        };
        Routines.Add(childTbl);
        return childTbl;
    }

    public void AddSymbol(string name, KarelSymbolKind kind, TokenPosition declarationPosition)
    {
        var symName = name.ToLower();
        if (!_symbols.ContainsKey(symName))
        {
            _symbols[symName] = new KarelSymbol(symName, kind, null, declarationPosition);
        }
    }

    public void AddSymbol(string name, KarelSymbolKind kind, KarelUserType type, TokenPosition declarationPosition)
    {
        var symName = name.ToLower();
        if (!_symbols.ContainsKey(symName))
        {
            _symbols[symName] = new KarelSymbol(symName, kind, type, declarationPosition);
        }
    }

    public void AddUsage(string name, TokenPosition usagePosition)
    {
        if (_symbols.TryGetValue(name.ToLower(), out var symbol))
        {
            symbol.UsagePositions.Add(usagePosition);
        }
    }

    public KarelSymbol? GetSymbol(string name, TokenPosition position)
    {
        var symName = name.ToLower();
        if (!IsPositionInScope(position))
        {
            return null;
        }

        foreach (var childTbl in Routines)
        {
            if (childTbl.GetSymbol(symName, position) is {} scoped)
            {
                return scoped;
            }
        }

        _symbols.TryGetValue(symName, out var symbol);
        return symbol;
    }

    public IEnumerable<KarelSymbol> GetAllSymbols() => _symbols.Values;

    private bool IsPositionInScope(TokenPosition position)
        => position.Line >= ScopeStart.Line
        && position.Line <= ScopeEnd.Line;
}

