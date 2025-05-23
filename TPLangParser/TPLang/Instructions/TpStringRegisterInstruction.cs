using Sprache;

namespace TPLangParser.TPLang.Instructions;

public record TpStringRegisterInstruction() : TpInstruction(0), ITpParser<TpStringRegisterInstruction>
{
    protected static readonly Parser<TpValueRegister> StringRegisterArg
        = TpValueRegister.StringRegister
            .Or(TpValueRegister.ArgumentRegister);

    protected static readonly Parser<TpValueRegister> StringRegisterValue
        = TpValueRegister.NumericRegister
            .Or(StringRegisterArg);

    protected static readonly Parser<TpValue> IndexValue
        = TpValueRegister.NumericRegister
            .Or(TpValueRegister.ArgumentRegister)
            .Or(TpValueIntegerConstant.GetParser());

    public new static Parser<TpStringRegisterInstruction> GetParser() 
        => TpStringRegisterConcatenation.GetParser()
            .Or(TpStringRegisterAssignment.GetParser())
            .Or(TpStringRegisterLength.GetParser())
            .Or(TpStringRegisterSearch.GetParser())
            .Or(TpStringRegisterCut.GetParser());
}

public sealed record TpStringRegisterAssignment(TpStringRegister StringRegister, TpValueRegister Value)
    : TpStringRegisterInstruction, ITpParser<TpStringRegisterInstruction>
{
    public new static Parser<TpStringRegisterInstruction> GetParser()
        => from stringRegister in TpStringRegister.GetParser()
            from sep in TpCommon.Keyword("=")
            from value in StringRegisterValue
            select new TpStringRegisterAssignment(stringRegister, value);
}

public sealed record TpStringRegisterConcatenation(TpStringRegister StringRegister, TpValueRegister Lhs, TpValueRegister Rhs)
    : TpStringRegisterInstruction, ITpParser<TpStringRegisterInstruction>
{
    public new static Parser<TpStringRegisterInstruction> GetParser()
        => from stringRegister in TpStringRegister.GetParser()
            from sep in TpCommon.Keyword("=")
            from lhs in StringRegisterValue
            from op in TpArithmeticOperatorParser.Plus
            from rhs in StringRegisterValue
            select new TpStringRegisterConcatenation(stringRegister, lhs, rhs);
}

public sealed record TpStringRegisterLength(TpRegister ResultRegister, TpStringRegister StringRegister)
    : TpStringRegisterInstruction, ITpParser<TpStringRegisterInstruction>
{
    public new static Parser<TpStringRegisterInstruction> GetParser()
        => from resultRegister in TpRegister.GetParser()
            from sep in TpCommon.Keyword("=")
            from keyword in TpCommon.Keyword("STRLEN")
            from stringRegister in TpStringRegister.GetParser()
            select new TpStringRegisterLength(resultRegister, stringRegister);
}

public sealed record TpStringRegisterSearch(
    TpRegister ResultRegister,
    TpValueRegister InputString,
    TpValueRegister SearchString)
    : TpStringRegisterInstruction, ITpParser<TpStringRegisterInstruction>
{
    public new static Parser<TpStringRegisterInstruction> GetParser()
        => from resultRegister in TpRegister.GetParser()
            from sep in TpCommon.Keyword("=")
            from keyword in TpCommon.Keyword("FINDSTR")
            from inputStr in StringRegisterArg
            from sep2 in TpCommon.Keyword(",")
            from searchStr in StringRegisterArg
            select new TpStringRegisterSearch(resultRegister, inputStr, searchStr);
}

public sealed record TpStringRegisterCut(
    TpStringRegister ResultRegister,
    TpValueRegister InputString,
    TpValue BeginIndex,
    TpValue EndIndex)
    : TpStringRegisterInstruction, ITpParser<TpStringRegisterInstruction>
{
    public new static Parser<TpStringRegisterInstruction> GetParser()
        => from resultRegister in TpStringRegister.GetParser()
           from sep in TpCommon.Keyword("=")
           from keyword in TpCommon.Keyword("SUBSTR")
           from inputStr in StringRegisterArg
           from sep2 in TpCommon.Keyword(",")
           from beginIndex in IndexValue
           from sep3 in TpCommon.Keyword(",")
           from endIndex in IndexValue
           select new TpStringRegisterCut(resultRegister, inputStr, beginIndex, endIndex);
}
