using Sprache;

namespace AGT.TPLangParser.TPLang.Instructions;

public record TpConditionMonitorInstruction() : TpInstruction(0), ITpParser<TpConditionMonitorInstruction>
{
    public new static Parser<TpConditionMonitorInstruction> GetParser()
        => TpMonitorEndInstruction.GetParser()
            .Or(TpMonitorInstruction.GetParser())
            .Or(TpWhenInstruction.GetParser());
}

public sealed record TpMonitorInstruction(string ProgramName)
    : TpConditionMonitorInstruction, ITpParser<TpConditionMonitorInstruction>
{
    public new static Parser<TpConditionMonitorInstruction> GetParser()
        => from keyword in TpCommon.Keyword("MONITOR")
            from programName in TpCommon.ProgramName
            select new TpMonitorInstruction(programName);
}

public sealed record TpMonitorEndInstruction(string ProgramName)
    : TpConditionMonitorInstruction, ITpParser<TpConditionMonitorInstruction>
{
    public new static Parser<TpConditionMonitorInstruction> GetParser()
        => from keyword in TpCommon.Keyword("MONITOR")
            from end in TpCommon.Keyword("END")
            from programName in TpCommon.ProgramName
            select new TpMonitorEndInstruction(programName);
}

// TODO: those don't belong in our programs anyway
public sealed record TpWhenCondition;

public sealed record TpWhenInstruction(TpWhenCondition Condition, string ProgramName)
    : TpConditionMonitorInstruction, ITpParser<TpConditionMonitorInstruction>
{
    public new static Parser<TpConditionMonitorInstruction> GetParser()
        => TpCommon.Fail<TpConditionMonitorInstruction>("Not Implemented");
}