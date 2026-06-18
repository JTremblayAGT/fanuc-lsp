using Sprache;

namespace TPLangParser.TPLang.Instructions;

public record TpMathInstruction(TpValue Variable, TpMathExpression Expression) : TpInstruction, ITpParser<TpMathInstruction>
{
    public new static Parser<TpMathInstruction> GetParser() 
        => from variable in TpValue.Assignable
            from sep in TpCommon.Keyword("=")
            from expression in TpMathExpression.GetParser()
            select new TpMathInstruction(variable, expression);
}

