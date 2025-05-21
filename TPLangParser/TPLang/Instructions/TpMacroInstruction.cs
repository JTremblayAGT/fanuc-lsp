using Sprache;

namespace AGT.TPLangParser.TPLang.Instructions;

public record TpMacroInstruction(string ProgramName) : TpInstruction(0), ITpParser<TpMacroInstruction>
{
    public new static Parser<TpMacroInstruction> GetParser() 
        => TpCommon.ProgramName.Select(name => new TpMacroInstruction(name));
}