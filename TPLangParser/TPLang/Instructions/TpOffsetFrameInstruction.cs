using Sprache;

namespace TPLangParser.TPLang.Instructions;

public record TpOffsetFrameInstruction() : TpInstruction(0), ITpParser<TpOffsetFrameInstruction>
{
    public new static Parser<TpOffsetFrameInstruction> GetParser() 
        => TpOffsetConditionInstruction.GetParser()
            .Or(TpUserFrameUseInstruction.GetParser())
            .Or(TpUserToolUseInstruction.GetParser())
            .Or(TpUserFrameSetInstruction.GetParser())
            .Or(TpUserToolSetInstruction.GetParser());
}

public sealed record TpOffsetConditionInstruction(TpPositionRegister PositionRegister, TpUserFrame? UserFrame)
    : TpOffsetFrameInstruction, ITpParser<TpOffsetFrameInstruction>
{
    public new static Parser<TpOffsetFrameInstruction> GetParser() 
        => from keyword in TpCommon.Keyword("OFFSET CONDITION")
            from posReg in TpPositionRegister.GetParser()
            from frame in (
                from keyword in TpCommon.Keyword(",")
                from userFrame in TpUserFrame.GetParser()
                select userFrame
            ).Optional()
            select new TpOffsetConditionInstruction(posReg, frame.GetOrDefault());
}

public sealed record TpUserFrameUseInstruction(int GroupNumber, TpValue Value) : TpOffsetFrameInstruction, ITpParser<TpOffsetFrameInstruction>
{
    public new static Parser<TpOffsetFrameInstruction> GetParser()
        => from keyword in TpCommon.Keyword("UFRAME_NUM")
            from groupNumber in (
                from gp in TpCommon.Keyword("GP")
                from num in Parse.Number.Select(int.Parse)
                select num
            ).BetweenBrackets().Optional()
            from sep in TpCommon.Keyword("=")
            from value in TpValueIntegerConstant.GetParser().Or(TpValueRegister.GetParser())
            select new TpUserFrameUseInstruction(groupNumber.GetOrElse(1), value);
}

public sealed record TpUserToolUseInstruction(int GroupNumber, TpValue Value) : TpOffsetFrameInstruction, ITpParser<TpOffsetFrameInstruction>
{
    public new static Parser<TpOffsetFrameInstruction> GetParser()
        => from keyword in TpCommon.Keyword("UTOOL_NUM")
            from groupNumber in (
                from gp in TpCommon.Keyword("GP")
                from num in Parse.Number.Select(int.Parse)
                select num
            ).BetweenBrackets().Optional()
            from sep in TpCommon.Keyword("=")
            from value in TpValueIntegerConstant.GetParser().Or(TpValueRegister.GetParser())
            select new TpUserToolUseInstruction(groupNumber.GetOrElse(1), value);
}

public sealed record TpUserFrameSetInstruction(TpUserFrame UserFrame, TpPositionRegister Value)
    : TpOffsetFrameInstruction, ITpParser<TpOffsetFrameInstruction>
{
    public new static Parser<TpOffsetFrameInstruction> GetParser() 
        => from uframe in TpUserFrame.GetParser()
            from sep in TpCommon.Keyword("=")
            from posReg in TpPositionRegister.GetParser()
            select new TpUserFrameSetInstruction(uframe, posReg);
}

public sealed record TpUserToolSetInstruction(TpUserTool UserTool, TpPositionRegister Value)
    : TpOffsetFrameInstruction, ITpParser<TpOffsetFrameInstruction>
{
    public new static Parser<TpOffsetFrameInstruction> GetParser() 
        => from utool in TpUserTool.GetParser()
            from sep in TpCommon.Keyword("=")
            from posReg in TpPositionRegister.GetParser()
            select new TpUserToolSetInstruction(utool, posReg);
}
