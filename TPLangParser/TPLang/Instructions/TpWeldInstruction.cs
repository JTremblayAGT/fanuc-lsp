using Sprache;

namespace AGT.TPLangParser.TPLang.Instructions;

public sealed record TpWeldInstruction(TpArcWeldingOptionType Type, TpWeldInstructionArgs Args)
    : TpInstruction(0), ITpParser<TpInstruction>
{
    public new static Parser<TpInstruction> GetParser()
        => from keyword in Parse.String("Arc").Token()
            from type in TpArcWeldingOptionTypeParser.Parser
            from args in TpWeldInstructionArgs.GetParser()
            select new TpWeldInstruction(type, args);
}
