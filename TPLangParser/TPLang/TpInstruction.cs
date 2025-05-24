using TPLangParser.TPLang.Instructions;
using Sprache;

namespace TPLangParser.TPLang;

public static class InstructionParsingExtensions
{
    public static Parser<(T Value, int LineNumber)> WithLineNumber<T>(this Parser<T> parser) where T : TpInstruction
        => input =>
        {
            var result = parser(input);
            if (result.WasSuccessful)
            {
                return Result.Success((result.Value, result.Remainder.Line), result.Remainder);
            }

            return Result.Failure<(T Value, int LineNumber)>(result.Remainder,
                $"Unexpected character '{result.Remainder.Current}'", []);
        };

    public static Parser<T> EnsureLineConsumed<T>(this Parser<T> parser) where T : TpInstruction
        => input =>
        {
            var result = parser(input);
            var lineEnd = TpCommon.LineEnd.Preview()(result.Remainder);
            if (result.WasSuccessful && lineEnd.Value.IsDefined)
            {
                return Result.Success(result.Value, result.Remainder);
            }

            return Result.Failure<T>(result.Remainder,
                $"Unexpected character '{result.Remainder.Current}'", []);
        };
}

public abstract record TpInstruction(int LineNumber) : ITpParser<TpInstruction>
{
    private static readonly Parser<TpInstruction> InternalParser
        /*
         *  .EnsureLineConsumed() needs to be added to every branch of the .Or() chain
         *  to ensure that a parser doesn't succeed with a partial parse of a line
         *  where another later in the chain would fully consume it
         *
         *  An instruction parser must fully consume a program line in order to succeed
         */
        = TpMotionInstruction.GetParser().EnsureLineConsumed()
            .Or(TpWeldInstruction.GetParser().EnsureLineConsumed())
            .Or(TpWeaveInstruction.GetParser().EnsureLineConsumed())
            .Or(TpBranchingInstruction.GetParser().EnsureLineConsumed())
            .Or(TpCollisionDetectInstruction.GetParser().EnsureLineConsumed())
            .Or(TpConditionMonitorInstruction.GetParser().EnsureLineConsumed())
            .Or(TpForInstruction.GetParser().EnsureLineConsumed())
            .Or(TpIOInstruction.GetParser().EnsureLineConsumed())
            .Or(TpMathInstruction.GetParser().EnsureLineConsumed())
            .Or(TpMiscInstruction.GetParser().EnsureLineConsumed())
            .Or(TpMixedLogicInstruction.GetParser().EnsureLineConsumed())
            .Or(TpOffsetFrameInstruction.GetParser().EnsureLineConsumed())
            .Or(TpPayloadInstruction.GetParser().EnsureLineConsumed())
            .Or(TpPosRegInstruction.GetParser().EnsureLineConsumed())
            .Or(TpProgramControlInstruction.GetParser().EnsureLineConsumed())
            .Or(TpRegisterInstruction.GetParser().EnsureLineConsumed())
            .Or(TpStringRegisterInstruction.GetParser().EnsureLineConsumed())
            .Or(TpSkipInstruction.GetParser().EnsureLineConsumed())
            .Or(TpWaitInstruction.GetParser().EnsureLineConsumed())
            .Or(TpInstructionComment.GetParser().EnsureLineConsumed())
            .Or(TpAutoBackwardExitInstruction.GetParser().EnsureLineConsumed())
            .Or(TpMultipleControlInstruction.GetParser().EnsureLineConsumed())
            // Try macro last (before generic) to ensure it doesn't wrongfully parse a single keyword instruction
            .Or(TpMacroInstruction.GetParser().EnsureLineConsumed())
            .Or(TpEmptyInstruction.GetParser().EnsureLineConsumed());

    public static Parser<TpInstruction> GetParser()
        => from lineNumber in TpCommon.LineNumber.Or(TpCommon.Fail<int>("Failed to parse start of line."))
           from kvp in InternalParser.WithLineNumber()
           from lineEnd in TpCommon.LineEnd
           select kvp.Value with { LineNumber = kvp.LineNumber };
}

public sealed record TpEmptyInstruction() : TpInstruction(0), ITpParser<TpEmptyInstruction>
{
    public new static Parser<TpEmptyInstruction> GetParser()
        => from whitespace in Parse.WhiteSpace.AtLeastOnce()
           select new TpEmptyInstruction();
}

public sealed record TpInstructionComment(string Comment) : TpInstruction(0), ITpParser<TpInstructionComment>
{
    private static readonly Parser<string> CommentParser =
        TpCommon.Keyword("!")
            .Or(TpCommon.Keyword("//"))
            .Then(_ => Parse.CharExcept(';').Many().Text());

    private static readonly Parser<string> MultiLineComment =
        from keyword in TpCommon.Keyword("--eg")
        from commentLines in (
            from lead in Parse.Char(':').Token()
            from comment in Parse.CharExcept(';').Many().Text()
            select comment
        ).DelimitedBy(Parse.Char(';'), 1, null)
        select commentLines.Aggregate((acc, com) => acc + com);


    public new static Parser<TpInstructionComment> GetParser()
        => from comment in CommentParser.Or(MultiLineComment)
           select new TpInstructionComment(comment);
}
