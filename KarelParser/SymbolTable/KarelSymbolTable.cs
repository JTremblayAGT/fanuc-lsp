using ParserUtils;

namespace KarelParser.SymTable;

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
        DeclarationPosition = declarationPosition;
        UsagePositions = new();
    }
}

public class KarelSymbolTable
{
    private readonly Dictionary<string, KarelSymbol> _symbols = new();

    public void AddSymbol(string name, KarelSymbolKind kind, TokenPosition declarationPosition)
    {
        if (!_symbols.ContainsKey(name))
        {
            _symbols[name] = new KarelSymbol(name, kind, null, declarationPosition);
        }
    }

    public void AddSymbol(string name, KarelSymbolKind kind, KarelUserType type, TokenPosition declarationPosition)
    {
        if (!_symbols.ContainsKey(name))
        {
            _symbols[name] = new KarelSymbol(name, kind, type, declarationPosition);
        }
    }

    public void AddUsage(string name, TokenPosition usagePosition)
    {
        if (_symbols.TryGetValue(name, out var symbol))
        {
            symbol.UsagePositions.Add(usagePosition);
        }
    }

    public KarelSymbol? GetSymbol(string name)
    {
        _symbols.TryGetValue(name, out var symbol);
        return symbol;
    }

    public IEnumerable<KarelSymbol> GetAllSymbols() => _symbols.Values;
}

