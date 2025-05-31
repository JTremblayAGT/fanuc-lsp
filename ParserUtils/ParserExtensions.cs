using Sprache;

namespace ParserUtils;

public sealed record TokenPosition(int Line, int Column);

public abstract record WithPosition
{
    public TokenPosition Start = new(0, 0);
    public TokenPosition End = new(0, 0);
};

public static class ParserExtensions
{
    public static Parser<string> Keyword(string keyword)
        => Parse.String(keyword).Text().Token();

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
                    (result.Value, new(input.Line, input.Column)),
                    result.Remainder
                );
            }

            return Result.Failure<(TParsedType Value, TokenPosition Position)>
                (input, $"Unexpected character '{result.Remainder.Current}'", []);
        };

    public static Parser<(TParsedType Value, TokenPosition Position)> WithEndPosition<TParsedType>(this Parser<TParsedType> parser)
        => input =>
        {
            var result = parser(input);
            if (result.WasSuccessful)
            {
                return Result.Success<(TParsedType Value, TokenPosition Position)>
                (
                    (result.Value, new(result.Remainder.Line, result.Remainder.Column)),
                    result.Remainder
                );
            }

            return Result.Failure<(TParsedType Value, TokenPosition Position)>
                (input, $"Unexpected character '{result.Remainder.Current}'", []);
        };

    public static Parser<(TParsedType Value, TokenPosition Start, TokenPosition End)> WithPosition<TParsedType>(
        this Parser<TParsedType> parser)
        => parser
            .WithStartPosition()
            .WithEndPosition()
            .Select(obj => (obj.Value.Value, obj.Value.Position, obj.Position));
}
