using Sprache;

namespace AGT.TPLangParser.TPLang.Instructions;

public record TpSkipInstruction() : TpInstruction(0), ITpParser<TpSkipInstruction>
{
    public new static Parser<TpSkipInstruction> GetParser() 
        => TpSkipCondition.GetParser();
}

public sealed record TpSkipCondition(TpLogicExpression Condition) : TpSkipInstruction, ITpParser<TpSkipInstruction>
{
    public new static Parser<TpSkipInstruction> GetParser() 
        => from keyword in TpCommon.Keyword("SKIP CONDITION")
            from condition in TpLogicExpression.GetParser()
            select new TpSkipCondition(condition);
}

