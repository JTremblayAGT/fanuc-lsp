using Sprache;

namespace AGT.TPLangParser.TPLang.Instructions;

public abstract record TpCollisionGuardInstruction() : TpInstruction(0), ITpParser<TpCollisionGuardInstruction>
{
    public new static Parser<TpCollisionGuardInstruction> GetParser() 
        => TpCollisionDetectInstruction.GetParser();
}

public sealed record TpCollisionDetectInstruction(TpOnOffState State) : TpCollisionGuardInstruction, ITpParser<TpCollisionDetectInstruction>
{
    public new static Parser<TpCollisionDetectInstruction> GetParser() 
        => from keyword in Parse.String("COL DETECT").Token()
            from state in TpOnOffStateParser.Parser
            select new TpCollisionDetectInstruction(state);
}
