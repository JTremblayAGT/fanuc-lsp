using Sprache;

namespace TPLangParser.TPLang.Instructions;

public record TpMacroInstruction(string ProgramName) : TpInstruction, ITpParser<TpMacroInstruction>
{
    public new static Parser<TpMacroInstruction> GetParser()
        => TpCommon.ProgramName.Select(name => new TpMacroInstruction(name));
}
