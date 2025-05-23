using Sprache;

namespace TPLangParser.TPLang.Instructions;

public enum TpForCountDirection
{
    Up, // TO
    Down, // DOWNTO
}

public struct TpForCountDirectionParser
{
    public static readonly Parser<TpForCountDirection> Parser
        = TpCommon.Keyword("TO").Return(TpForCountDirection.Up)
            .Or(TpCommon.Keyword("DOWNTO").Return(TpForCountDirection.Down));
}

public record TpForInstruction() : TpInstruction(0), ITpParser<TpForInstruction>
{
    public new static Parser<TpForInstruction> GetParser() 
        => TpBeginForInstruction.GetParser()
            .Or(TpEndForInstruction.GetParser());
}

public record TpBeginForInstruction(
    TpRegister Counter,
    TpValue InitialValue,
    TpValue TargetValue,
    TpForCountDirection CountDirection) : TpForInstruction, ITpParser<TpForInstruction>
{
    private static readonly Parser<TpValue> AllowedValues
        = TpValueIntegerConstant.GetParser()
            .Or(TpValueRegister.GetParser());

    public new static Parser<TpForInstruction> GetParser()
        => from keyword in TpCommon.Keyword("FOR")
            from counter in TpRegister.GetParser()
            from sep in TpCommon.Keyword("=")
            from initialValue in AllowedValues
            from direction in TpForCountDirectionParser.Parser
            from targetValue in AllowedValues
            select new TpBeginForInstruction(counter, initialValue, targetValue, direction);
}

public record TpEndForInstruction : TpForInstruction, ITpParser<TpForInstruction>
{
    public new static Parser<TpForInstruction> GetParser()
        => TpCommon.Keyword("ENDFOR").Return(new TpEndForInstruction());
}
