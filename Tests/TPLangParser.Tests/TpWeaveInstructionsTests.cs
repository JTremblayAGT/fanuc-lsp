using AGT.TPLangParser.TPLang;
using AGT.TPLangParser.TPLang.Instructions;
using Sprache;

namespace AGT.TPLangParser.Tests;
public class TpWeaveInstructionsTests
{
    [Theory]
    [InlineData("Weave Sine[1]", TpWeavePattern.Sine, 1)]
    [InlineData("Weave Figure 8[5]", TpWeavePattern.Figure8, 5)]
    [InlineData("Weave Circle[10]", TpWeavePattern.Circle, 10)]
    [InlineData("Weave Sine 2[15]", TpWeavePattern.Sine2, 15)]
    [InlineData("Weave L[20]", TpWeavePattern.L, 20)]
    public void ParseWeaveInstruction_WithDirectSchedule_ParsesCorrectly(
        string input,
        TpWeavePattern expectedPattern,
        int expectedSchedule)
    {
        var result = TpWeaveInstruction.GetParser().Parse(input) as TpWeaveStartInstruction;

        Assert.NotNull(result);
        Assert.Equal(expectedPattern, result.Type);

        Assert.IsType<TpWeldInstructionWeldSchedule>(result.Args);
        var schedule = (TpWeldInstructionWeldSchedule)result.Args;
        Assert.Equal(expectedSchedule, schedule.Access.Number);
    }

    [Theory]
    [InlineData("Weave Sine[R[1]]", TpWeavePattern.Sine, 1)]
    [InlineData("Weave Figure 8[R[5]]", TpWeavePattern.Figure8, 5)]
    [InlineData("Weave Circle[R[10]]", TpWeavePattern.Circle, 10)]
    [InlineData("Weave Sine 2[R[15]]", TpWeavePattern.Sine2, 15)]
    [InlineData("Weave L[R[20]]", TpWeavePattern.L, 20)]
    public void ParseWeaveInstruction_WithRegisterSchedule_ParsesCorrectly(
        string input,
        TpWeavePattern expectedPattern,
        int expectedRegisterNumber)
    {
        var result = TpWeaveInstruction.GetParser().Parse(input) as TpWeaveStartInstruction;

        Assert.NotNull(result);
        Assert.Equal(expectedPattern, result.Type);

        Assert.IsType<TpWeldInstructionRegister>(result.Args);
        var register = (TpWeldInstructionRegister)result.Args;
        Assert.IsType<TpRegister>(register.Register);

        var directAccess = (TpAccessDirect)register.Register.Access;
        Assert.Equal(expectedRegisterNumber, directAccess.Number);
    }

    [Theory]
    [InlineData("Weave Sine[1.5,2.0,3.2]", TpWeavePattern.Sine, 3)]
    [InlineData("Weave Figure 8[0.5,1.0,1.5,2.0]", TpWeavePattern.Figure8, 4)]
    [InlineData("Weave Circle[1.5,2.0,3.2,4.0,5.0]", TpWeavePattern.Circle, 5)]
    [InlineData("Weave Sine 2[0,0.0mm,0.000,0.00,0.0s]", TpWeavePattern.Sine2, 5)]
    [InlineData("Weave L[1.1,2,3,4.5]", TpWeavePattern.L, 4)]
    public void ParseWeaveInstruction_WithParameters_ParsesCorrectly(
        string input,
        TpWeavePattern expectedPattern,
        int expectedParameterCount)
    {
        var result = TpWeaveInstruction.GetParser().Parse(input) as TpWeaveStartInstruction;

        Assert.NotNull(result);
        Assert.Equal(expectedPattern, result.Type);

        Assert.IsType<TpWeldInstructionParameters>(result.Args);
        var parameters = (TpWeldInstructionParameters)result.Args;

        Assert.NotNull(parameters.Parameters);
        Assert.Equal(expectedParameterCount, parameters.Parameters.Count);
    }

    [Theory]
    [InlineData("Weave End", null)]
    [InlineData("Weave End[1]", 1)]
    [InlineData("Weave End[5]", 5)]
    [InlineData("Weave End[10]", 10)]
    public void ParseWeaveEndInstruction_ParsesCorrectly(string input, int? expectedSchedule)
    {
        var result = TpWeaveInstruction.GetParser().Parse(input) as TpWeaveEndInstruction;

        Assert.NotNull(result);

        if (expectedSchedule.HasValue)
        {
            Assert.NotNull(result.Schedule);
            Assert.Equal(expectedSchedule.Value, result.Schedule.Access.Number);
        }
        else
        {
            Assert.Null(result.Schedule);
        }
    }

    [Theory]
    [InlineData("Weave[1]")]
    [InlineData("Weave Square[5]")]
    [InlineData("Weaving Sine[1]")]
    [InlineData("WEAVE sine[5]")]
    public void ParseWeaveInstruction_InvalidFormat_ThrowsParseException(string input) 
        => Assert.Throws<ParseException>(() => TpWeaveInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("Weaving End[5]")]
    [InlineData("WEAVE END")]
    [InlineData("weave end[5]")]
    public void ParseWeaveEndInstruction_InvalidFormat_ThrowsParseException(string input) 
        => Assert.Throws<ParseException>(() => TpWeaveInstruction.GetParser().Parse(input));
}
