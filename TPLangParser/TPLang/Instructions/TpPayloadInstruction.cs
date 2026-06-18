using Sprache;

namespace TPLangParser.TPLang.Instructions;

public sealed record TpPayloadInstruction(TpAccess Access) : TpInstruction, ITpParser<TpPayloadInstruction>
{
    public new static Parser<TpPayloadInstruction> GetParser() 
        => from keyword in TpCommon.Keyword("PAYLOAD")
            from access in TpAccess.GetParser()
            select new TpPayloadInstruction(access);
}
