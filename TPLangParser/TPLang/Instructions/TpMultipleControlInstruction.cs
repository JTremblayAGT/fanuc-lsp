using Sprache;

using ParserUtils;

namespace TPLangParser.TPLang.Instructions;

public abstract record TpMultipleControlInstruction() : TpInstruction(0), ITpParser<TpMultipleControlInstruction>
{
    public new static Parser<TpMultipleControlInstruction> GetParser()
        => TpRunInstruction.GetParser();
}

public sealed record TpRunInstruction(TpCallByName ProgramName)
    : TpMultipleControlInstruction, ITpParser<TpMultipleControlInstruction>
{
    public new static Parser<TpMultipleControlInstruction> GetParser()
        => from keyword in TpCommon.Keyword("RUN")
           from programName in TpCallByName.GetParser().WithPosition()
           select new TpRunInstruction((TpCallByName)programName.Value with
           {
               Start = programName.Start,
               End = programName.End
           });
}
