using ParserUtils;

using Sprache;

namespace KarelParser;

internal interface IKarelParser<out TParsedType>
{
    public static abstract Parser<TParsedType> GetParser();
}

public class KarelCommon
{
    public static Parser<string> Identifier
        => Parse.Identifier(Parse.Letter, Parse.LetterOrDigit.Or(Parse.Char('_')))
            .Token()
            .Then(ident => ident switch
            {
                { Length: <= 12} => Parse.Return(ident),
                _ => input => Result.Failure<string>(input,
                    $"Identifier '{ident}' has more than 12 characters.",
                    [])
            });
}

public record KarelValue : WithPosition, IKarelParser<KarelValue>
{
    public static Parser<KarelValue> GetParser()
        => KarelString.GetParser()
            .Or(KarelInteger.GetParser());
}

public sealed record KarelString(string Value) : KarelValue, IKarelParser<KarelValue>
{
    public new static Parser<KarelValue> GetParser()
        => Parse.AnyChar.Many()
            .Contained(Parse.Char('\''), Parse.Char('\''))
            .Text()
            .Token()
            .WithPosition()
            .Select(res => new KarelString(res.Value) { Start = res.Start, End = res.End });
}

public sealed record KarelInteger(int Value) : KarelValue, IKarelParser<KarelValue>
{
    public new static Parser<KarelValue> GetParser()
        => from negated in ParserUtils.ParserExtensions.Keyword("-").Optional()
            from num in Parse.Number.Select(int.Parse)
            select new KarelInteger(negated switch
            {
                { IsDefined: true } => -num,
                _ => num
            });
}
