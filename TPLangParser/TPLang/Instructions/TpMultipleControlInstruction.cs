using Sprache;

namespace TPLangParser.TPLang.Instructions;

public abstract record TpMultipleControlInstruction() : TpInstruction(0), ITpParser<TpMultipleControlInstruction>
{
    public new static Parser<TpMultipleControlInstruction> GetParser() 
        => TpRunInstruction.GetParser();
}

public sealed record TpRunInstruction(string ProgramName)
    : TpMultipleControlInstruction, ITpParser<TpMultipleControlInstruction>
{
    public new static Parser<TpMultipleControlInstruction> GetParser() 
        => from keyword in TpCommon.Keyword("RUN")
            from programName in TpCommon.ProgramName
            select new TpRunInstruction(programName);
}
