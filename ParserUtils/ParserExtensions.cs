using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

using Sprache;

namespace ParserUtils;

public sealed record TokenPosition(int Line, int Column)
{
    public override string ToString()
        => $"L.{Line}:{Column}";

    public static TokenPosition NullPos = new(-1,-1);
}

public abstract record WithPosition
{
    public TokenPosition Start = TokenPosition.NullPos;
    public TokenPosition End = TokenPosition.NullPos;

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    // False when Start/End were never populated by the parser. A genuinely
    public bool HasPosition
        => !(Start == TokenPosition.NullPos && End == TokenPosition.NullPos);

    // True when position falls within [Start, End] inclusive.
    public bool Contains(TokenPosition position)
        => IsAtOrAfterStart(position) && IsAtOrBeforeEnd(position);

    private bool IsAtOrAfterStart(TokenPosition position)
        => position.Line > Start.Line
           || (position.Line == Start.Line && position.Column >= Start.Column);

    private bool IsAtOrBeforeEnd(TokenPosition position)
        => position.Line < End.Line
           || (position.Line == End.Line && position.Column <= End.Column);

    // Recursively descends the AST and returns the innermost node whose
    // [Start, End] range contains the position. A positioned node is ignored
    // entirely if the position is outside its range; if no child claims the
    // position the node returns itself (the base case for leaves and for
    // parents whose children all declined).
    //
    // Nodes the parser never assigned a position to (HasPosition == false) are
    // traversed transparently: they neither gate descent nor claim a match, so
    // their positioned children remain reachable.
    public virtual TNodeType? GetNodeAt<TNodeType>(TokenPosition position) where TNodeType : class
    {
        if (HasPosition && !Contains(position))
        {
            return null;
        }

        foreach (var child in ChildNodes())
        {
            if (child.GetNodeAt<TNodeType>(position) is TNodeType match)
            {
                return match;
            }
        }

        return HasPosition && this is TNodeType node ? node : null;
    }

    // The immediate positioned children of this node, found by walking the
    // record's properties and passing transparently through any wrapper values
    // (collections, dictionaries, non-positioned records) until a WithPosition
    // boundary is reached.
    private IEnumerable<WithPosition> ChildNodes()
    {
        foreach (var property in PropertiesOf(GetType()))
        {
            object? value;
            try
            {
                value = property.GetValue(this);
            }
            catch
            {
                continue;
            }

            foreach (var node in PositionedDescendants(value))
            {
                yield return node;
            }
        }
    }

    private static IEnumerable<WithPosition> PositionedDescendants(object? value)
    {
        switch (value)
        {
            case null:
            case string:
            case TokenPosition:
                yield break;
            // A positioned node is a boundary: yield it and let its own
            // GetNodeAt handle descending further.
            case WithPosition node:
                yield return node;
                yield break;
            case IDictionary dictionary:
                foreach (var entry in dictionary.Values)
                {
                    foreach (var node in PositionedDescendants(entry))
                    {
                        yield return node;
                    }
                }
                yield break;
            case IEnumerable sequence:
                foreach (var element in sequence)
                {
                    foreach (var node in PositionedDescendants(element))
                    {
                        yield return node;
                    }
                }
                yield break;
        }

        var type = value.GetType();
        if (type.IsPrimitive || type.IsEnum || value is decimal)
        {
            yield break;
        }

        // A non-positioned wrapper record: descend through its properties.
        foreach (var property in PropertiesOf(type))
        {
            object? child;
            try
            {
                child = property.GetValue(value);
            }
            catch
            {
                continue;
            }

            foreach (var node in PositionedDescendants(child))
            {
                yield return node;
            }
        }
    }

    private static PropertyInfo[] PropertiesOf(Type type)
        => PropertyCache.GetOrAdd(type, static t => t
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0 && p.CanRead)
            .ToArray());
};

public static class ParserExtensions
{
    public static Parser<string> Keyword(string keyword)
        => Parse.IgnoreCase(keyword).Text().Token();

    public static Parser<TParsedType> BetweenParen<TParsedType>(this Parser<TParsedType> parser)
        => parser.Contained(Keyword("("), Keyword(")"));

    public static Parser<TParsedType> BetweenBrackets<TParsedType>(this Parser<TParsedType> parser)
        => parser.Contained(Keyword("["), Keyword("]"));

    public static Parser<TParsedType> BetweenBraces<TParsedType>(this Parser<TParsedType> parser)
        => parser.Contained(Keyword("{"), Keyword("}"));

    public static Parser<(TParsedType Value, TokenPosition Position)> WithStartPosition<TParsedType>(this Parser<TParsedType> parser)
        => input =>
        {
            var result = parser(input);
            if (result.WasSuccessful)
            {
                return Result.Success<(TParsedType Value, TokenPosition Position)>
                (
                    (result.Value, new(input.Line - 1, input.Column - 1)),
                    result.Remainder
                );
            }

            var message = result.Remainder.AtEnd
                ? "Unexpected end of input"
                : result.Message;

            return Result.Failure<(TParsedType Value, TokenPosition Position)>
                (result.Remainder, message, result.Expectations);
        };

    public static Parser<(TParsedType Value, TokenPosition Position)> WithEndPosition<TParsedType>(this Parser<TParsedType> parser)
        => input =>
        {
            var result = parser(input);
            if (result.WasSuccessful)
            {
                return Result.Success<(TParsedType Value, TokenPosition Position)>
                (
                    (result.Value, new(result.Remainder.Line - 1, result.Remainder.Column - 1)),
                    result.Remainder
                );
            }

            var message = result.Remainder.AtEnd
                ? "Unexpected end of input"
                : result.Message;

            return Result.Failure<(TParsedType Value, TokenPosition Position)>
                (result.Remainder, message, result.Expectations);
        };

    public static Parser<(TParsedType Value, TokenPosition Start, TokenPosition End)> WithPosition<TParsedType>(
        this Parser<TParsedType> parser)
        => parser
            .WithStartPosition()
            .WithEndPosition()
            .Select(obj => (obj.Value.Value, obj.Value.Position, obj.Position));

    public static Parser<TParsedType> WithPos<TParsedType>(this Parser<TParsedType> parser)
        where TParsedType : WithPosition
        => parser
            .WithPosition()
            .Select(result => result.Value with
            {
                Start = result.Start,
                End = result.End
            });
}
