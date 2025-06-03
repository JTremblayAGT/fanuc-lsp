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

public enum KarelComparisonOperator
{
    Equal,    // =
    NotEqual, // <>
    Lesser,   // <
    LesserEq, // <=
    Greater,  // >
    GreaterEq, // >=
    PosApprox // >=< ???
}

public struct KarelComparisonOperatorParser
{
    public static Parser<KarelComparisonOperator> Parser()
        => KarelCommon.Keyword("=").Return(KarelComparisonOperator.Equal)
            .Or(KarelCommon.Keyword("<>").Return(KarelComparisonOperator.NotEqual))
            .Or(KarelCommon.Keyword("<=").Return(KarelComparisonOperator.LesserEq))
            .Or(KarelCommon.Keyword(">=").Return(KarelComparisonOperator.GreaterEq))
            .Or(KarelCommon.Keyword("<").Return(KarelComparisonOperator.Lesser))
            .Or(KarelCommon.Keyword(">").Return(KarelComparisonOperator.Greater));
}

public struct KarelSumOperatorParser
{
    public static Parser<KarelComparisonOperator> Parser()
        => KarelCommon.Keyword(">=<").Return(KarelComparisonOperator.PosApprox)
            .Or(KarelComparisonOperatorParser.Parser());
}

public enum KarelPositionOperator
{
    Relative, // :
    DotProd, // @
    CrossProd, // #
}

public struct KarelPositionOperatorParser
{
    public static Parser<KarelPositionOperator> Parser()
        => KarelCommon.Keyword(":").Return(KarelPositionOperator.Relative)
            .Or(KarelCommon.Keyword("@").Return(KarelPositionOperator.DotProd))
            .Or(KarelCommon.Keyword("#").Return(KarelPositionOperator.CrossProd));
}

public record KarelLabel(string Name) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from ident in KarelCommon.Identifier
           from kw in KarelCommon.Keyword("::")
           select new KarelLabel(ident);
}


