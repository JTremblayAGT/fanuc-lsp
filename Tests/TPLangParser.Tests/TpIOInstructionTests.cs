using AGT.TPLangParser.TPLang.Instructions;
using Sprache;

namespace AGT.TPLangParser.Tests;

public class TpIOInstructionTests
{
    [Theory]
    [InlineData("DO[1]=ON")]
    [InlineData("DO[10]=OFF")]
    [InlineData("DO[1]=PULSE")]
    [InlineData("DO[5]=DI[1]")]
    [InlineData("R[1]=DI[1]")]
    public void Parse_DigitalIOInstruction_ParsesCorrectly(string input)
    {
        var result = TpIOInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpDigitalIOInstruction>(result);

        var ioInst = (TpDigitalIOInstruction)result;
        Assert.NotNull(ioInst.Lhs);
        Assert.NotNull(ioInst.Rhs);
    }

    [Theory]
    [InlineData("RO[1]=ON")]
    [InlineData("RO[10]=OFF")]
    [InlineData("RO[1]=PULSE")]
    [InlineData("RO[5]=RI[1]")]
    [InlineData("R[1]=RI[1]")]
    public void Parse_RobotIOInstruction_ParsesCorrectly(string input)
    {
        var result = TpIOInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpRobotIOInstruction>(result);

        var ioInst = (TpRobotIOInstruction)result;
        Assert.NotNull(ioInst.Lhs);
        Assert.NotNull(ioInst.Rhs);
    }

    [Theory]
    [InlineData("AO[1]=1.5")]
    [InlineData("AO[10]=R[1]")]
    [InlineData("AO[5]=AI[1]")]
    [InlineData("R[1]=AI[1]")]
    public void Parse_AnalogIOInstruction_ParsesCorrectly(string input)
    {
        var result = TpIOInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpAnalogIOInstruction>(result);

        var ioInst = (TpAnalogIOInstruction)result;
        Assert.NotNull(ioInst.Lhs);
        Assert.NotNull(ioInst.Rhs);
    }

    [Theory]
    [InlineData("GO[1]=1.5")]
    [InlineData("GO[10]=R[1]")]
    [InlineData("GO[5]=GI[1]")]
    [InlineData("R[1]=GI[1]")]
    public void Parse_GroupIOInstruction_ParsesCorrectly(string input)
    {
        var result = TpIOInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpGroupIOInstruction>(result);

        var ioInst = (TpGroupIOInstruction)result;
        Assert.NotNull(ioInst.Lhs);
        Assert.NotNull(ioInst.Rhs);
    }

    [Theory]
    [InlineData("WO[1]=ON")]
    [InlineData("WO[10]=OFF")]
    [InlineData("WO[1]=PULSE")]
    [InlineData("WO[5]=WI[1]")]
    [InlineData("R[1]=WI[1]")]
    public void Parse_WeldingIOInstruction_ParsesCorrectly(string input)
    {
        var result = TpIOInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpWeldingIOInstruction>(result);

        var ioInst = (TpWeldingIOInstruction)result;
        Assert.NotNull(ioInst.Lhs);
        Assert.NotNull(ioInst.Rhs);
    }

    [Theory]
    [InlineData("DO[-1]=OFF")] // Negative port number
    [InlineData("DO[]=ON")] // Missing port number
    [InlineData("DO[1]=INVALID")] // Invalid state
    [InlineData("DO[1]=")] // Missing value
    [InlineData("=ON")] // Missing port
    [InlineData("DO[1]ON")] // Missing equals
    [InlineData("DOI[1]=ON")] // Invalid IO type
    public void Parse_InvalidIOInstruction_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpIOInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("DO[1] = ON")] // Spaces around equals
    [InlineData("DO[1]=  ON")] // Extra space before value
    [InlineData("DO[1]=ON  ")] // Extra space after value
    [InlineData("DO[1]\t=\tON")] // Tabs
    public void Parse_IOInstruction_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpIOInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("do[1]=ON")] // Lowercase DO
    [InlineData("Do[1]=ON")] // Mixed case DO
    [InlineData("DO[1]=on")] // Lowercase ON
    [InlineData("DO[1]=Off")] // Mixed case OFF
    [InlineData("RO[1]=Pulse")] // Mixed case PULSE
    public void Parse_IOInstruction_CaseInsensitive_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpIOInstruction.GetParser().Parse(input));
}
