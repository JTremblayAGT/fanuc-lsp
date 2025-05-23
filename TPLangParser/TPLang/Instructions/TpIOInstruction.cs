using Sprache;

namespace TPLangParser.TPLang.Instructions;

public record TpIOInstruction() : TpInstruction(0), ITpParser<TpIOInstruction>
{
    public new static Parser<TpIOInstruction> GetParser()
        => TpDigitalIOInstruction.GetParser()
            .Or(TpRobotIOInstruction.GetParser())
            .Or(TpAnalogIOInstruction.GetParser())
            .Or(TpGroupIOInstruction.GetParser())
            .Or(TpWeldingIOInstruction.GetParser());
}

public record TpDigitalIOInstruction(TpValue Lhs, TpValue Rhs) : TpIOInstruction, ITpParser<TpIOInstruction>
{
    private static readonly Parser<TpValue> LhsValue
        = TpValueRegister.NumericRegister.Select(TpValue (regVal) => regVal)
            .Or(TpValueIOPort.MakeParser<TpDigitalIOPort>(TpDigitalIOPort.Prefix(), TpIOType.Output))
            .Token();

    private static readonly Parser<TpValue> RhsValue
        = TpValueIOPort.MakeParser<TpDigitalIOPort>(TpDigitalIOPort.Prefix(), TpIOType.Input)
            .Or(TpValueIOState.GetParser())
            .Or(TpValueRegister.NumericRegister)
            .Or(TpValuePulse.GetParser())
            .Token();

    public new static Parser<TpIOInstruction> GetParser()
        => from lhs in LhsValue
            from sep in Parse.Char('=')
            from rhs in RhsValue
            select new TpDigitalIOInstruction(lhs, rhs);
}

public record TpRobotIOInstruction(TpValue Lhs, TpValue Rhs) : TpIOInstruction, ITpParser<TpIOInstruction>
{
    private static readonly Parser<TpValue> LhsValue
        = TpValueRegister.NumericRegister.Select(TpValue (regVal) => regVal)
            .Or(TpValueIOPort.MakeParser<TpRobotIOPort>(TpRobotIOPort.Prefix(), TpIOType.Output))
            .Token();

    private static readonly Parser<TpValue> RhsValue
        = TpValueIOPort.MakeParser<TpRobotIOPort>(TpRobotIOPort.Prefix(), TpIOType.Input)
            .Or(TpValueIOState.GetParser())
            .Or(TpValueRegister.NumericRegister)
            .Or(TpValuePulse.GetParser())
            .Token();

    public new static Parser<TpIOInstruction> GetParser()
        => from lhs in LhsValue
            from sep in Parse.Char('=')
            from rhs in RhsValue
            select new TpRobotIOInstruction(lhs, rhs);
}

public record TpAnalogIOInstruction(TpValue Lhs, TpValue Rhs) : TpIOInstruction, ITpParser<TpIOInstruction>
{
    private static readonly Parser<TpValue> LhsValue
        = TpValueRegister.NumericRegister.Select(TpValue (regVal) => regVal)
            .Or(TpValueIOPort.MakeParser<TpAnalogIOPort>(TpAnalogIOPort.Prefix(), TpIOType.Output))
            .Token();

    private static readonly Parser<TpValue> RhsValue
        = TpValueIOPort.MakeParser<TpAnalogIOPort>(TpAnalogIOPort.Prefix(), TpIOType.Input)
            .Or(TpValueFloatingPointConstant.GetParser())
            .Or(TpValueRegister.NumericRegister)
            .Token();

    public new static Parser<TpIOInstruction> GetParser()
        => from lhs in LhsValue
            from sep in Parse.Char('=')
            from rhs in RhsValue
            select new TpAnalogIOInstruction(lhs, rhs);
}

public record TpGroupIOInstruction(TpValue Lhs, TpValue Rhs) : TpIOInstruction, ITpParser<TpIOInstruction>
{
    private static readonly Parser<TpValue> LhsValue
        = TpValueRegister.NumericRegister.Select(TpValue (regVal) => regVal)
            .Or(TpValueIOPort.MakeParser<TpGroupIOPort>(TpGroupIOPort.Prefix(), TpIOType.Output))
            .Token();

    private static readonly Parser<TpValue> RhsValue
        = TpValueIOPort.MakeParser<TpGroupIOPort>(TpGroupIOPort.Prefix(), TpIOType.Input)
            .Or(TpValueFloatingPointConstant.GetParser())
            .Or(TpValueRegister.NumericRegister)
            .Token();

    public new static Parser<TpIOInstruction> GetParser()
        => from lhs in LhsValue
            from sep in Parse.Char('=')
            from rhs in RhsValue
            select new TpGroupIOInstruction(lhs, rhs);
}

public record TpWeldingIOInstruction(TpValue Lhs, TpValue Rhs) : TpIOInstruction, ITpParser<TpIOInstruction>
{
    private static readonly Parser<TpValue> LhsValue
        = TpValueRegister.NumericRegister.Select(TpValue (regVal) => regVal)
            .Or(TpValueIOPort.MakeParser<TpWeldingIOPort>(TpWeldingIOPort.Prefix(), TpIOType.Output))
            .Token();

    private static readonly Parser<TpValue> RhsValue
        = TpValueIOPort.MakeParser<TpWeldingIOPort>(TpWeldingIOPort.Prefix(), TpIOType.Input)
            .Or(TpValueIOState.GetParser())
            .Or(TpValueRegister.NumericRegister)
            .Or(TpValuePulse.GetParser())
            .Token();

    public new static Parser<TpIOInstruction> GetParser()
        => from lhs in LhsValue
            from sep in Parse.Char('=')
            from rhs in RhsValue
            select new TpWeldingIOInstruction(lhs, rhs);
}
