using Sprache;

namespace TPLangParser.TPLang.Instructions;

public enum TpWeavePattern
{
    Sine,
    Figure8,
    Circle,
    Sine2,
    L
}

internal struct TpWeavePatternParser
{
    public static readonly Parser<TpWeavePattern> Parser =
        TpCommon.Keyword("Sine 2").Return(TpWeavePattern.Sine2)
            .Or(TpCommon.Keyword("Figure 8").Return(TpWeavePattern.Figure8))
            .Or(TpCommon.Keyword("Circle").Return(TpWeavePattern.Circle))
            .Or(TpCommon.Keyword("Sine").Return(TpWeavePattern.Sine))
            .Or(TpCommon.Keyword("L").Return(TpWeavePattern.L))
            .Token();
}

public sealed record TpWeaveStartInstruction(TpWeavePattern Type, TpWeldInstructionArgs Args)
    : TpWeaveInstruction, ITpParser<TpWeaveInstruction>
{
    public new static Parser<TpWeaveInstruction> GetParser()
        => from keyword in TpCommon.Keyword("Weave").Token()
            from pattern in TpWeavePatternParser.Parser
            from args in TpWeldInstructionArgs.GetParser()
            select new TpWeaveStartInstruction(pattern, args);
}

public sealed record TpWeaveEndInstruction(TpWeldInstructionWeldSchedule? Schedule)
    : TpWeaveInstruction, ITpParser<TpWeaveInstruction>
{
    public new static Parser<TpWeaveInstruction> GetParser()
        => from keyword in TpCommon.Keyword("Weave End")
            from schedule in TpWeldInstructionArgs.GetParser().Optional()
            select new TpWeaveEndInstruction(
                schedule.IsDefined ? schedule.Get() as TpWeldInstructionWeldSchedule : null);
}

public record TpWeaveInstruction() : TpInstruction(0), ITpParser<TpInstruction>
{
    public new static Parser<TpInstruction> GetParser()
        => TpWeaveStartInstruction.GetParser()
            .Or(TpWeaveEndInstruction.GetParser());
}