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

public record KarelSymbol
{
    public string Name { get; init; }

    // The dotted path that addresses this symbol from a program-level variable
    // (without the program name), e.g. "Var.Field1.Field2". This mirrors the way
    // TP programs reference Karel data ($[PROG]Var.Field1.Field2), so it is the
    // key used to find a Karel symbol's references in TP files. For symbols that
    // aren't reached through a variable (types, routines, per-TYPE fields), it is
    // simply the symbol name.
    public string FullName { get; init; }
    public KarelSymbolKind Kind { get; init; }
    public TokenPosition DeclarationPosition { get; init; }
    public List<TokenPosition> ReferencePositions { get; init; }
    public KarelUserType? Type { get; init; }

    public KarelSymbol(string name, KarelSymbolKind kind, KarelUserType? type, TokenPosition declarationPosition)
        : this(name, name, kind, type, declarationPosition)
    {
    }

    public KarelSymbol(string name, string fullName, KarelSymbolKind kind, KarelUserType? type, TokenPosition declarationPosition)
    {
        Name = name;
        FullName = fullName;
        Kind = kind;
        Type = type;
        DeclarationPosition = declarationPosition;
        ReferencePositions = new();
    }
}

// KarelSymbolTable needs to be a recursive structure to properly represent lexical scoping
public class KarelSymbolTable
{
    private ReaderWriterLockSlim Lock { get; } = new();

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
        => LockedWrite(() => {
            var symName = name.ToLower();
            if (!_symbols.ContainsKey(symName))
            {
                _symbols[symName] = new KarelSymbol(symName, kind, null, declarationPosition);
            }
        });

    public void AddSymbol(string name, KarelSymbolKind kind, KarelUserType? type, TokenPosition declarationPosition)
        => LockedWrite(() => {
            var symName = name.ToLower();
            if (!_symbols.ContainsKey(symName))
            {
                _symbols[symName] = new KarelSymbol(symName, kind, type, declarationPosition);
            }
        });

    public void AddReference(string name, TokenPosition refPosition)
        => LockedWrite(() => {
            if (_symbols.TryGetValue(name.ToLower(), out var symbol))
            {
                symbol.ReferencePositions.Add(refPosition);
            }

            if (Parent?.GetTopLevelSymbol(name) is { } parentSym)
            {
                parentSym.ReferencePositions.Add(refPosition);
            }
        });

    // Registers a variable or one of its reachable struct fields under its
    // fully-qualified path. First registration wins, mirroring AddSymbol.
    public void AddQualifiedSymbol(string fullName, string name, KarelSymbolKind kind, KarelUserType? type, TokenPosition declarationPosition)
        => LockedWrite(() =>
        {
            var key = name.ToLower();
            if (!_symbols.ContainsKey(key))
            {
                _symbols[key] = new(name, fullName, kind, type, declarationPosition);
            }
            else
            {
                _symbols[key] = _symbols[key] with { FullName = fullName };
            }
        });

    public KarelSymbol? GetTopLevelSymbol(string name)
        => LockedRead(() => _symbols.GetValueOrDefault(name.ToLower()));

    public KarelSymbol? GetSymbol(string name, TokenPosition position)
        => LockedRead<KarelSymbol?>(() => {
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

            return _symbols.GetValueOrDefault(symName);
        });

    public List<TokenPosition> GetSymbolReferences(string name)
        => GetTopLevelSymbol(name) switch
        {
            { } symbol => symbol.ReferencePositions,
            _ => []
        };

    public List<TokenPosition> GetSymbolReferences(string name, TokenPosition position)
        => GetSymbol(name, position) switch
        {
            { } symbol => symbol.ReferencePositions,
            _ => []
        };

    public IEnumerable<KarelSymbol> GetAllSymbols()
        => LockedRead(() => _symbols.Values);

    private bool IsPositionInScope(TokenPosition position)
        => position.Line >= ScopeStart.Line
        && position.Line <= ScopeEnd.Line;

    private void LockedWrite(Action func)
    {
        try
        {
            Lock.EnterWriteLock();
            func();
        }
        catch
        {
            throw;
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
        catch
        {
            throw;
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

}

