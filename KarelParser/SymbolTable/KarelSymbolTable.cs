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
    public List<TokenPosition> ReferencePositions { get; }
    public KarelUserType? Type { get; }

    public KarelSymbol(string name, KarelSymbolKind kind, KarelUserType? type, TokenPosition declarationPosition)
    {
        Name = name;
        Kind = kind;
        Type = type;
        DeclarationPosition = declarationPosition;
        ReferencePositions = new();
    }
}

// KarelSymbolTable needs to be a recursive structure to properly represent lexical scoping
public class KarelSymbolTable
{
    private KarelSymbolTable? Parent { get; set; } = null;
    //
    // There should never be more than one level of children realistically
    private List<KarelSymbolTable> Routines { get; set;} = [];

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
            Parent = this,
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

    public void AddSymbol(string name, KarelSymbolKind kind, KarelUserType? type, TokenPosition declarationPosition)
    {
        var symName = name.ToLower();
        if (!_symbols.ContainsKey(symName))
        {
            _symbols[symName] = new KarelSymbol(symName, kind, type, declarationPosition);
        }
    }

    public void AddReference(string name, TokenPosition refPosition)
    {
        if (_symbols.TryGetValue(name.ToLower(), out var symbol))
        {
            symbol.ReferencePositions.Add(refPosition);
        }

        if (Parent?.GetSymbol(name, refPosition) is { } parentSym)
        {
            parentSym.ReferencePositions.Add(refPosition);
        }
    }

    public KarelSymbol? GetSymbol(string name)
    {
        var symName = name.ToLower();
        _symbols.TryGetValue(symName, out var symbol);
        return symbol;
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

    public List<TokenPosition> GetSymbolReferences(string name, TokenPosition position)
        => GetSymbol(name, position) switch
        {
            { } symbol => symbol.ReferencePositions,
            _ => []
        };

    public IEnumerable<KarelSymbol> GetAllSymbols() => _symbols.Values;

    private bool IsPositionInScope(TokenPosition position)
        => position.Line >= ScopeStart.Line
        && position.Line <= ScopeEnd.Line;
}

