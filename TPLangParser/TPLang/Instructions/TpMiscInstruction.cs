using Sprache;

namespace TPLangParser.TPLang.Instructions;

public record TpMiscInstruction() : TpInstruction(0), ITpParser<TpMiscInstruction>
{
    public new static Parser<TpMiscInstruction> GetParser()
        => TpRsrInstruction.GetParser()
            .Or(TpUserAlarmInstruction.GetParser())
            .Or(TpTimerInstruction.GetParser())
            .Or(TpOverrideInstruction.GetParser())
            .Or(TpMessageInstruction.GetParser())
            .Or(TpParameterWriteInstruction.GetParser())
            .Or(TpParameterReadInstruction.GetParser())
            .Or(TpJointMaxSpeedInstruction.GetParser())
            .Or(TpLinearMaxSpeedInstruction.GetParser());
}

public record TpRsrInstruction(TpAccess Access, bool Enable) : TpMiscInstruction, ITpParser<TpMiscInstruction>
{
    public new static Parser<TpMiscInstruction> GetParser()
        => from keyword in TpCommon.Keyword("RSR")
           from access in TpAccess.GetParser()
           from enable in (
               TpCommon.Keyword("ENABLE").Return(true)
                   .Or(TpCommon.Keyword("DISABLE").Return(false))
           )
           select new TpRsrInstruction(access, enable);
}

public record TpUserAlarmInstruction(TpAccess Access) : TpMiscInstruction, ITpParser<TpMiscInstruction>
{
    public new static Parser<TpMiscInstruction> GetParser()
        => from keyword in TpCommon.Keyword("UALM")
           from access in TpAccess.GetParser()
           select new TpUserAlarmInstruction(access);
}

public enum TpTimerAction
{
    Start,
    Stop,
    Reset
}

public struct TpTimerActionParser
{
    public static readonly Parser<TpTimerAction> Parser
        = TpCommon.Keyword("START").Return(TpTimerAction.Start)
            .Or(TpCommon.Keyword("STOP").Return(TpTimerAction.Stop))
            .Or(TpCommon.Keyword("RESET").Return(TpTimerAction.Reset));

}

public record TpTimerInstruction(TpAccess Access, TpTimerAction Action) : TpMiscInstruction, ITpParser<TpMiscInstruction>
{
    public new static Parser<TpMiscInstruction> GetParser()
        => from keyword in TpCommon.Keyword("TIMER")
           from access in TpAccess.GetParser()
           from sep in TpCommon.Keyword("=")
           from action in TpTimerActionParser.Parser
           select new TpTimerInstruction(access, action);
}

public abstract record TpOverrideInstruction : TpMiscInstruction, ITpParser<TpMiscInstruction>
{
    public new static Parser<TpMiscInstruction> GetParser()
        => TpOverrideDirect.GetParser()
            .Or(TpOverrideIndirect.GetParser());
}

public record TpOverrideDirect(int Value) : TpMiscInstruction, ITpParser<TpMiscInstruction>
{
    public new static Parser<TpMiscInstruction> GetParser()
        => from keyword in TpCommon.Keyword("OVERRIDE")
           from sep in TpCommon.Keyword("=")
           from value in Parse.Number.Select(int.Parse)
           from tail in TpCommon.Keyword("%")
           select new TpOverrideDirect(value);
}

public record TpOverrideIndirect(TpValueRegister Register) : TpMiscInstruction, ITpParser<TpMiscInstruction>
{
    public new static Parser<TpMiscInstruction> GetParser()
        => from keyword in TpCommon.Keyword("OVERRIDE")
           from sep in TpCommon.Keyword("=")
           from value in TpValueRegister.NumericRegister.Or(TpValueRegister.ArgumentRegister)
           select new TpOverrideIndirect(value);
}

public record TpMessageInstruction(string Message) : TpMiscInstruction, ITpParser<TpMiscInstruction>
{
    public new static Parser<TpMiscInstruction> GetParser()
        => from keyword in TpCommon.Keyword("MESSAGE")
           from message in Parse.AnyChar.Until(input =>
           {
               var result = Parse.Char(']').Preview()(input);
               if (!result.Value.IsDefined)
               {
                   return Result.Failure<char>(input, string.Empty, []);
               }
               var next = result.Remainder.Advance();

               // Try parsing anything that can come after a register comment
               var lookAhead = TpCommon.LineEnd.Preview()(next);
               return lookAhead.Value.IsDefined || next.AtEnd ? Result.Success(']', input) : Result.Failure<char>(input, string.Empty, []);
           }).Text().BetweenBrackets()
           select new TpMessageInstruction(message);
}

public record TpParameterWriteInstruction(TpValueParameter Parameter, TpValue Value) : TpMiscInstruction, ITpParser<TpMiscInstruction>
{
    public new static Parser<TpMiscInstruction> GetParser()
        => from param in TpValueParameter.GetParser()
           from sep in TpCommon.Keyword("=")
           from value in TpValue.GetParser()
           select new TpParameterWriteInstruction(param, value);
}

public record TpParameterReadInstruction(TpRegister Register, TpValueParameter Parameter) : TpMiscInstruction, ITpParser<TpMiscInstruction>
{
    public new static Parser<TpMiscInstruction> GetParser()
        => from reg in TpRegister.GetParser().Or(TpArgumentRegister.GetParser())
           from sep in TpCommon.Keyword("=")
           from param in TpValueParameter.GetParser()
           select new TpParameterReadInstruction(reg, param);
}

public record TpJointMaxSpeedInstruction(TpAccess Access, TpValue Value) : TpMiscInstruction, ITpParser<TpMiscInstruction>
{
    public new static Parser<TpMiscInstruction> GetParser()
        => from keyword in TpCommon.Keyword("JOINT_MAX_SPEED")
           from access in TpAccess.GetParser()
           from sep in TpCommon.Keyword("=")
           from value in
               TpValueRegister.GetParser()
                   .Or(TpValueFloatingPointConstant.GetParser())
                   .Or(TpValueIntegerConstant.GetParser())
           select new TpJointMaxSpeedInstruction(access, value);
}

public record TpLinearMaxSpeedInstruction(TpAccess Access, TpValue Value) : TpMiscInstruction, ITpParser<TpMiscInstruction>
{
    public new static Parser<TpMiscInstruction> GetParser()
        => from keyword in TpCommon.Keyword("LINEAR_MAX_SPEED")
           from access in TpAccess.GetParser()
           from sep in TpCommon.Keyword("=")
           from value in
               TpValueRegister.GetParser()
                   .Or(TpValueFloatingPointConstant.GetParser())
                   .Or(TpValueIntegerConstant.GetParser())
           select new TpLinearMaxSpeedInstruction(access, value);
}
