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

// Identifies a register or IO port within its kind's table: the literal index,
// the optional motion group (0 = none) and — for IO ports only — the signal
// direction. Registers always use Input and ignore Direction.
public readonly record struct TpSymbolIndex(int Number, int Group = 0, TpIOType Direction = TpIOType.Input);

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

// Registers and IO ports are stored in one index-keyed dictionary per kind, and
// system/Karel variables in one name-keyed dictionary per kind. Looking a symbol
// up therefore needs no string formatting — the common case (resolving a clicked
// register across every open TP program for "find references") is a plain int
// dictionary hit per program once the clicked token has been parsed once.
public sealed class TpSymbolTable
{
    private ReaderWriterLockSlim Lock { get; } = new();

    private static readonly TpSymbolKind[] IndexedKinds =
    [
        TpSymbolKind.NumReg, TpSymbolKind.PosReg, TpSymbolKind.StrReg, TpSymbolKind.ArgReg,
        TpSymbolKind.DigitalIO, TpSymbolKind.RobotIO, TpSymbolKind.SopIO, TpSymbolKind.UopIO,
        TpSymbolKind.AnalogIO, TpSymbolKind.GroupIO, TpSymbolKind.WeldIO
    ];

    private static readonly TpSymbolKind[] NamedKinds =
    [
        TpSymbolKind.SysVar, TpSymbolKind.KarelVar
    ];

    private readonly Dictionary<TpSymbolKind, Dictionary<TpSymbolIndex, TpSymbol>> _indexed
        = IndexedKinds.ToDictionary(kind => kind, _ => new Dictionary<TpSymbolIndex, TpSymbol>());

    private readonly Dictionary<TpSymbolKind, Dictionary<string, TpSymbol>> _named
        = NamedKinds.ToDictionary(kind => kind, _ => new Dictionary<string, TpSymbol>());

    // Registers and IO ports are never declared — they spring into existence the
    // first time they're read or written. Record the usage and create the symbol
    // (formatting its display name once) on first sight.
    public void RecordIndexedUsage(TpSymbolKind kind, TpSymbolIndex index, TpSymbolRefKind refKind, TokenPosition position, object node)
        => LockedWrite(() =>
        {
            var map = _indexed[kind];
            if (!map.TryGetValue(index, out var symbol))
            {
                symbol = new TpSymbol(FormatIndexedName(kind, index), kind, node);
                map[index] = symbol;
            }

            symbol.Usages.Add(new TpSymbolReference { Position = position, Kind = refKind });
        });

    // System and Karel variables are keyed by their (case-insensitive) source
    // name, e.g. "$ERROR" or "$[PROG]var.field".
    public void RecordNamedUsage(TpSymbolKind kind, string name, TpSymbolRefKind refKind, TokenPosition position, object node)
        => LockedWrite(() =>
        {
            var map = _named[kind];
            var key = name.ToLowerInvariant();
            if (!map.TryGetValue(key, out var symbol))
            {
                symbol = new TpSymbol(name, kind, node);
                map[key] = symbol;
            }

            symbol.Usages.Add(new TpSymbolReference { Position = position, Kind = refKind });
        });

    public TpSymbol? GetIndexedSymbol(TpSymbolKind kind, TpSymbolIndex index)
        => LockedRead(() => _indexed.TryGetValue(kind, out var map) ? map.GetValueOrDefault(index) : null);

    public TpSymbol? GetNamedSymbol(TpSymbolKind kind, string name)
        => LockedRead(() => _named.TryGetValue(kind, out var map) ? map.GetValueOrDefault(name.ToLowerInvariant()) : null);

    // Resolve a symbol from its canonical display name ("R[5]", "DO[1]",
    // "$ERROR", "$[PROG]var"). Indirectly-indexed names (R[R[2]]) and unknown
    // forms resolve to null. For repeated cross-program lookups, parse once with
    // TryResolveKey and call GetIndexedSymbol/GetNamedSymbol directly.
    public TpSymbol? GetSymbol(string name)
        => TryResolveKey(name, out var kind, out var index, out var namedKey) switch
        {
            true when namedKey is not null => GetNamedSymbol(kind, namedKey),
            true => GetIndexedSymbol(kind, index),
            _ => null
        };

    public IEnumerable<TpSymbol> GetAllSymbols()
        => LockedRead(() => _indexed.Values.SelectMany(map => map.Values)
            .Concat(_named.Values.SelectMany(map => map.Values))
            .ToList());

    public List<TokenPosition> GetSymbolReferences(string name)
        => GetSymbol(name) switch
        {
            { } symbol => symbol.Usages.Select(usage => usage.Position).ToList(),
            _ => []
        };

    // Parses a canonical symbol name into its storage key. Returns false for
    // names that aren't recordable symbols (indirect indices, unknown prefixes).
    // On success exactly one of `index` (registers/IO) or `namedKey`
    // (system/Karel variables) is meaningful — `namedKey` is non-null for the
    // named kinds.
    public static bool TryResolveKey(string name, out TpSymbolKind kind, out TpSymbolIndex index, out string? namedKey)
    {
        kind = default;
        index = default;
        namedKey = null;

        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        if (name.StartsWith("$["))
        {
            kind = TpSymbolKind.KarelVar;
            namedKey = name;
            return true;
        }

        if (name.StartsWith('$'))
        {
            kind = TpSymbolKind.SysVar;
            namedKey = name;
            return true;
        }

        var open = name.IndexOf('[');
        if (open < 0 || !name.EndsWith(']'))
        {
            return false;
        }

        if (!TryPrefixToKind(name[..open], out kind, out var direction)
            || !TryParseInner(name[(open + 1)..^1], out var number, out var group))
        {
            return false;
        }

        index = new TpSymbolIndex(number, group, direction);
        return true;
    }

    private static bool IsIoKind(TpSymbolKind kind)
        => kind is >= TpSymbolKind.DigitalIO and <= TpSymbolKind.WeldIO;

    private static string FormatIndexedName(TpSymbolKind kind, TpSymbolIndex index)
    {
        var group = index.Group > 0 ? $"GP{index.Group}:" : string.Empty;
        return IsIoKind(kind)
            ? $"{IoPrefix(kind)}{(index.Direction == TpIOType.Input ? "I" : "O")}[{group}{index.Number}]"
            : $"{RegisterPrefix(kind)}[{group}{index.Number}]";
    }

    private static string RegisterPrefix(TpSymbolKind kind)
        => kind switch
        {
            TpSymbolKind.ArgReg => "AR",
            TpSymbolKind.StrReg => "SR",
            TpSymbolKind.PosReg => "PR",
            _ => "R"
        };

    private static string IoPrefix(TpSymbolKind kind)
        => kind switch
        {
            TpSymbolKind.DigitalIO => "D",
            TpSymbolKind.RobotIO => "R",
            TpSymbolKind.SopIO => "S",
            TpSymbolKind.UopIO => "U",
            TpSymbolKind.AnalogIO => "A",
            TpSymbolKind.GroupIO => "G",
            TpSymbolKind.WeldIO => "W",
            _ => "?"
        };

    private static bool TryPrefixToKind(string prefix, out TpSymbolKind kind, out TpIOType direction)
    {
        direction = TpIOType.Input;
        (kind, direction) = prefix.ToUpperInvariant() switch
        {
            "R" => (TpSymbolKind.NumReg, direction),
            "AR" => (TpSymbolKind.ArgReg, direction),
            "SR" => (TpSymbolKind.StrReg, direction),
            "PR" => (TpSymbolKind.PosReg, direction),
            "DI" => (TpSymbolKind.DigitalIO, TpIOType.Input),
            "DO" => (TpSymbolKind.DigitalIO, TpIOType.Output),
            "RI" => (TpSymbolKind.RobotIO, TpIOType.Input),
            "RO" => (TpSymbolKind.RobotIO, TpIOType.Output),
            "SI" => (TpSymbolKind.SopIO, TpIOType.Input),
            "SO" => (TpSymbolKind.SopIO, TpIOType.Output),
            "UI" => (TpSymbolKind.UopIO, TpIOType.Input),
            "UO" => (TpSymbolKind.UopIO, TpIOType.Output),
            "AI" => (TpSymbolKind.AnalogIO, TpIOType.Input),
            "AO" => (TpSymbolKind.AnalogIO, TpIOType.Output),
            "GI" => (TpSymbolKind.GroupIO, TpIOType.Input),
            "GO" => (TpSymbolKind.GroupIO, TpIOType.Output),
            "WI" => (TpSymbolKind.WeldIO, TpIOType.Input),
            "WO" => (TpSymbolKind.WeldIO, TpIOType.Output),
            _ => ((TpSymbolKind)(-1), direction)
        };

        return (int)kind >= 0;
    }

    // Parses the inside of the brackets: an optional "GP{group}:" prefix followed
    // by the literal index. Anything else (an indirect index such as "R[2]") fails.
    private static bool TryParseInner(string inner, out int number, out int group)
    {
        group = 0;
        var rest = inner;
        if (inner.StartsWith("GP", StringComparison.OrdinalIgnoreCase))
        {
            var colon = inner.IndexOf(':');
            if (colon < 0 || !int.TryParse(inner[2..colon], out group))
            {
                number = 0;
                return false;
            }

            rest = inner[(colon + 1)..];
        }

        return int.TryParse(rest, out number);
    }

    private void LockedWrite(Action func)
    {
        try
        {
            Lock.EnterWriteLock();
            func();
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    private T LockedRead<T>(Func<T> func)
    {
        try
        {
            Lock.EnterReadLock();
            return func();
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }
}
