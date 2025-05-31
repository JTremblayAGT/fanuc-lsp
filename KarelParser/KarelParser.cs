using ParserUtils;

using Sprache;

namespace KarelParser;

internal interface IKarelParser<TParsedType>
{
    public static abstract Parser<TParsedType> GetParser();
}

internal static class KarelParserExtensions
{
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

// TODO: Comments need to be removed from buffer before parsing.
// The Doc Comment at the beginning must be parsed before that.
public class KarelCommon
{
    public static Parser<string> Identifier
        => Parse.Identifier(Parse.Letter, Parse.LetterOrDigit.Or(Parse.Char('_')))
            .Token()
            .Then(ident => ident switch
            {
                { Length: <= 12 } => Parse.Return(ident),
                _ => input => Result.Failure<string>(input,
                    $"Identifier '{ident}' has more than 12 characters.",
                    [])
            });

    public static Parser<string> LineBreak
        => Parse.LineEnd.Token();

    public static Parser<string> Keyword(string kw)
        => ParserUtils.ParserExtensions.Keyword(kw);

}

public record KarelLabel(string Name) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from ident in KarelCommon.Identifier
           from kw in KarelCommon.Keyword("::")
           select new KarelLabel(ident);
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

public sealed record KarelSystemIndentifier : KarelValue, IKarelParser<KarelValue>
{
    public new static Parser<KarelValue> GetParser()
        => throw new NotImplementedException();
}

// TODO: maybe a value?
public record KarelVariableAcess() : WithPosition, IKarelParser<KarelVariableAcess>
{
    public static Parser<KarelVariableAcess> GetParser()
        => throw new NotImplementedException();
}

public sealed record KarelGlobalCondition : WithPosition, IKarelParser<KarelGlobalCondition>
{
    public static Parser<KarelGlobalCondition> GetParser()
    {
        throw new NotImplementedException();
    }
}

public sealed record KarelAction : WithPosition, IKarelParser<KarelAction>
{
    public static Parser<KarelAction> GetParser()
    {
        throw new NotImplementedException();
    }
}
