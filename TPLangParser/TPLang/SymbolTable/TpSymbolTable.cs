using ParserUtils;

namespace TPLangParser.TPLang.SymbolTable;

public enum TpSymbolKind
{
    NumReg,
    PosReg,
    StrReg,
    ArgReg,
    DigitalIO,
    RobotIO,
    SopIO,
    UopIO,
    AnalogIO,
    GroupIO,
    WeldIO,
    SysVar,
    KarelVar
}

public enum TpSymbolRefKind
{
    Read,
    Write
}

public record struct TpSymbolReference
{
    public required TokenPosition Position { get; init; }
    public required TpSymbolRefKind Kind { get; init; }
}

public sealed class TpSymbol
{
    public string Name { get; }
    public TpSymbolKind Kind { get; }

    // A representative AST node for this symbol (the first occurrence seen).
    // Registers and IO ports are never declared in a TP program, so there is
    // no canonical "declaration" — this is just somewhere to point hover at.
    public object Symbol { get; }
    public List<TpSymbolReference> Usages { get; }

    public TpSymbol(string name, TpSymbolKind kind, object symbol)
    {
        Name = name;
        Kind = kind;
        Symbol = symbol;
        Usages = new();
    }

    public IEnumerable<TpSymbolReference> Reads
        => Usages.Where(usage => usage.Kind == TpSymbolRefKind.Read);

    public IEnumerable<TpSymbolReference> Writes
        => Usages.Where(usage => usage.Kind == TpSymbolRefKind.Write);
}

public sealed class TpSymbolTable
{
    private readonly Dictionary<string, TpSymbol> _symbols = new();

    // Symbols are never declared in a TP program — they spring into existence
    // the first time they're read or written. Record the usage and create the
    // symbol on first sight.
    public void RecordUsage(string name, TpSymbolKind kind, TpSymbolRefKind refKind, TokenPosition position, object node)
    {
        var key = name.ToLowerInvariant();
        if (!_symbols.TryGetValue(key, out var symbol))
        {
            symbol = new TpSymbol(name, kind, node);
            _symbols[key] = symbol;
        }

        symbol.Usages.Add(new TpSymbolReference { Position = position, Kind = refKind });
    }

    public TpSymbol? GetSymbol(string name)
        => _symbols.GetValueOrDefault(name.ToLowerInvariant());

    public IEnumerable<TpSymbol> GetAllSymbols() => _symbols.Values;

    public List<TokenPosition> GetSymbolReferences(string name)
        => GetSymbol(name) switch
        {
            { } symbol => symbol.Usages.Select(usage => usage.Position).ToList(),
            _ => []
        };
}