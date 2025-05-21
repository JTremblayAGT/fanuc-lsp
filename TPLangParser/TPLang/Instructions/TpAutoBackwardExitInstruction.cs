using Sprache;

namespace AGT.TPLangParser.TPLang.Instructions;

public abstract record TpAutoBackwardExitInstruction() : TpInstruction(0), ITpParser<TpAutoBackwardExitInstruction>
{
    protected static readonly Parser<string> LeadingKeyword
        = TpCommon.Keyword("Rec Path");

    public new static Parser<TpAutoBackwardExitInstruction> GetParser() 
        => from leading in LeadingKeyword
            from instruction in 
                TpCommon.Keyword("Start").Return((TpAutoBackwardExitInstruction)new TpRecPathStartInstruction())
                .Or(TpCommon.Keyword("End").Return(new TpRecPathEndInstruction()))
            select instruction;
}

public sealed record TpRecPathStartInstruction : TpAutoBackwardExitInstruction;

public sealed record TpRecPathEndInstruction : TpAutoBackwardExitInstruction;
