using Sprache;

namespace TPLangParser.TPLang.Instructions;

public record TpRegisterInstruction() : TpInstruction(0), ITpParser<TpRegisterInstruction>
{
    public new static Parser<TpRegisterInstruction> GetParser() 
        => TpRegisterAssignment.GetParser();
}

public sealed record TpRegisterAssignment(TpRegister Register, TpArithmeticExpression Expression)
    : TpRegisterInstruction, ITpParser<TpRegisterInstruction>
{
    public new static Parser<TpRegisterInstruction> GetParser() 
        => from register in TpRegister.GetParser()
            from sep in TpCommon.Keyword("=")
            from expr in TpArithmeticExpression.GetParser()
            select new TpRegisterAssignment(register, expr);
}