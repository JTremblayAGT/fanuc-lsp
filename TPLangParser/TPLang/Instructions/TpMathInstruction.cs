using Sprache;

namespace AGT.TPLangParser.TPLang.Instructions;

public record TpMathInstruction(TpValue Variable, TpMathExpression Expression) : TpInstruction(0), ITpParser<TpMathInstruction>
{
    public new static Parser<TpMathInstruction> GetParser() 
        => from variable in TpValue.Assignable
            from sep in TpCommon.Keyword("=")
            from expression in TpMathExpression.GetParser()
            select new TpMathInstruction(variable, expression);
}

