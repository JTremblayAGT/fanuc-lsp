using ParserUtils;

using Sprache;

namespace KarelParser;

internal interface IKarelParser<out TParsedType>
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

    public static Parser<T> WithErrorContext<T>(this Parser<T> parser, string contextName)
        => input =>
        {
            var result = parser(input);
            if (!result.WasSuccessful)
            {
                // Create a more descriptive error message that includes context
                return Result.Failure<T>(
                    result.Remainder,
                    $"Error in {contextName} (l:{input.Line} c:{input.Column}): {result.Message}",
                    result.Expectations);
            }
            return result;
        };
}

public class KarelCommon
{
    public static Parser<IEnumerable<KarelStatement>> ParseStatements(string[] endToken)
        => input =>
        {
            var statements = new List<KarelStatement>();
            var remainder = input;

            while (true)
            {
                if (endToken.Any(tok => Keyword(tok).Preview()(remainder).Value.IsDefined)
                    && !Parse.Ref(KarelStatement.GetParser).Preview()(remainder).Value.IsDefined)
                {
                    break;
                }

                var result = Parse.Ref(KarelStatement.GetParser)(remainder);
                if (!result.WasSuccessful)
                {
                    return Result.Failure<IEnumerable<KarelStatement>>(remainder, result.Message, result.Expectations);
                }

                statements.Add(result.Value);
                var sep = LineBreak.AtLeastOnce()(result.Remainder);
                remainder = sep.Remainder;
            }

            return Result.Success<IEnumerable<KarelStatement>>(statements, remainder);
        };

    // Complete list of Fanuc Karel programming language keywords
    private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "ABORT", "ABOUT", "ABS", "AFTER", "ALONG", "ALSO", "AND", "ARRAY", "ARRAY_LEN", "AT", "ATTACH", "AWAY", "AXIS",
        "BEFORE", "BEGIN", "BOOLEAN", "BY", "BYNAME", "BYTE",
        "CAM_SETUP", "CANCEL", "CASE", "CLOSE", "CMOS", "COMMAND", "COMMON_ASSOC", "CONDITION", "CONFIG", "CONNECT", "CONST", "CONTINUE", "COORDINATED", "CR",
        "DELAY", "DISABLE", "DISCONNECT", "DIV", "DO", "DOWNTO", "DRAM",
        "ELSE", "ENABLE", "END", "ENDCONDITION", "ENDFOR", "ENDIF", "ENDMOVE", "ENDSELECT", "ENDSTRUCTURE", "ENDUSING", "ENDWHILE", "ERROR", "EVAL", "EVENT", "END",
        "FALSE", "FILE", "FOR", "FROM",
        "GET_VAR", "GO", "GOTO", "GROUP", "GROUP_ASSOC",
        "HAND", "HOLD",
        "IF", "IN", "INDEPENDENT", "INTEGER",
        "JOINTPOS", "JOINTPOS1", "JOINTPOS2", "JOINTPOS3", "JOINTPOS4", "JOINTPOS5", "JOINTPOS6", "JOINTPOS7", "JOINTPOS8", "JOINTPOS9",
        "MOD", "MODEL", "MOVE",
        "NEAR", "NOABORT", "NODE", "NODEDATA", "NOMESSAGE", "NOPAUSE", "NOT", "NOWAIT",
        "OF", "OPEN", "OR",
        "PATH", "PATHHEADER", "PAUSE", "POSITION", "POWERUP", "PROGRAM", "PULSE", "PURGE",
        "READ", "REAL", "RELATIVE", "RELEASE", "RELAX", "REPEAT", "RESTORE", "RESUME", "RETURN", "ROUTINE",
        "SELECT", "SEMAPHORE", "SET_VAR", "SHORT", "SIGNAL", "STOP", "STRING", "STRUCTURE",
        "THEN", "TIME", "TIMER", "TO", "TPENABLE", "TRUE", "TYPE",
        "UNHOLD", "UNINIT", "UNPAUSE", "UNTIL", "USING",
        "VAR", "VECTOR", "VIA", "VIS_PROCESS",
        "WAIT", "WHEN", "WHILE", "WITH", "WRITE",
        "XYZWPR", "XYZWPREXT"
    };

    private static readonly HashSet<string> Intrinsics = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET_VAR", "SET_VAR", "UNINIT", "ARRAY_LEN", "EVAL"
    };

    public static Parser<string> Identifier
        => Parse.Identifier(Parse.Letter.Or(Parse.Char('$')), Parse.LetterOrDigit.Or(Parse.Char('_')))
            .Token()
            .Then(ident => Keywords.Contains(ident.ToUpperInvariant())
                ? input => Result.Failure<string>(input,
                    $"'{ident}' is a reserved keyword and cannot be used as an identifier.",
                    ["identifier"])
                : Parse.Return(ident));
    public static Parser<string> Intrinsic
        => Parse.Identifier(Parse.Letter, Parse.LetterOrDigit.Or(Parse.Char('_')))
            .Token()
            .Then(ident => Intrinsics.Contains(ident.ToUpperInvariant())
                ? Parse.Return(ident)
                : input => Result.Failure<string>(input,
                    $"'{ident}' is not an intrinsic function",
                    ["keyword"])
                    );

    public static Parser<string> Reserved
        => Parse.Identifier(Parse.Letter, Parse.LetterOrDigit.Or(Parse.Char('_')))
            .Token()
            .Then(ident => Keywords.Contains(ident.ToUpperInvariant())
                ? Parse.Return(ident)
                : input => Result.Failure<string>(input,
                    $"'{ident}' is not a reserved keyword",
                    ["keyword"])
                    );

    public static Parser<string> LineBreak
        => Parse.LineEnd.Or(Keyword(";"));

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

public struct KarelExprOperatorParser
{
    public static Parser<KarelComparisonOperator> Parser()
        => KarelCommon.Keyword(">=<").Return(KarelComparisonOperator.PosApprox)
            .Or(KarelComparisonOperatorParser.Parser());
}

public enum KarelPositionOperator
{
    Relative,  // :
    DotProd,   // @
    CrossProd, // #
}

public struct KarelPositionOperatorParser
{
    public static Parser<KarelPositionOperator> Parser()
        => KarelCommon.Keyword(":").Return(KarelPositionOperator.Relative)
            .Or(KarelCommon.Keyword("@").Return(KarelPositionOperator.DotProd))
            .Or(KarelCommon.Keyword("#").Return(KarelPositionOperator.CrossProd));
}

public enum KarelProductOperator
{
    Times, // *
    Slash, // /
    And,   // AND
    Div,   // DIV
    Mod    // MOD
}

public struct KarelProductOperatorParser
{
    public static Parser<KarelProductOperator> Parser()
        => KarelCommon.Keyword("*").Return(KarelProductOperator.Times)
            .Or(KarelCommon.Keyword("/").Return(KarelProductOperator.Slash))
            .Or(KarelCommon.Keyword("AND").Return(KarelProductOperator.And))
            .Or(KarelCommon.Keyword("DIV").Return(KarelProductOperator.Div))
            .Or(KarelCommon.Keyword("MOD").Return(KarelProductOperator.Mod));
}

public enum KarelSumOperator
{
    Plus,   // +
    Minus,  // -
    Or      // OR
}

public struct KarelSumOperatorParser
{
    public static Parser<KarelSumOperator> Parser()
        => KarelCommon.Keyword("+").Return(KarelSumOperator.Plus)
            .Or(KarelCommon.Keyword("-").Return(KarelSumOperator.Minus))
            .Or(KarelCommon.Keyword("OR").Return(KarelSumOperator.Or));
}

public record KarelLabel(string Name) : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from ident in KarelCommon.Identifier
           from kw in KarelCommon.Keyword("::")
           select new KarelLabel(ident);
}

