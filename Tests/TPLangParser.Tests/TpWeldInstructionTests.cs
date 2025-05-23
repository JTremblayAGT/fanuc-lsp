using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;
using Sprache;

namespace TPLangParser.Tests;

public class TpWeldInstructionTests
{
    [Theory]
    [InlineData("Arc Start[1]", TpArcWeldingOptionType.Start, 1)]
    [InlineData("Arc End[5]", TpArcWeldingOptionType.End, 5)]
    public void ParseWeldInstruction_WithDirectSchedule_ParsesCorrectly(
        string input,
        TpArcWeldingOptionType expectedType,
        int expectedSchedule)
    {
        var result = TpWeldInstruction.GetParser().Parse(input) as TpWeldInstruction;

        Assert.NotNull(result);
        Assert.Equal(expectedType, result.Type);

        Assert.IsType<TpWeldInstructionWeldSchedule>(result.Args);
        var schedule = (TpWeldInstructionWeldSchedule)result.Args;
        Assert.Equal(expectedSchedule, schedule.Access.Number);
    }

    [Theory]
    [InlineData("Arc Start[R[1]]", TpArcWeldingOptionType.Start, 1)]
    [InlineData("Arc End[R[5]]", TpArcWeldingOptionType.End, 5)]
    [InlineData("Arc Start[AR[1]]", TpArcWeldingOptionType.Start, 1)]
    [InlineData("Arc End[AR[5]]", TpArcWeldingOptionType.End, 5)]
    public void ParseWeldInstruction_WithRegisterSchedule_ParsesCorrectly(
        string input,
        TpArcWeldingOptionType expectedType,
        int expectedRegisterNumber)
    {
        var result = TpWeldInstruction.GetParser().Parse(input) as TpWeldInstruction;

        Assert.NotNull(result);
        Assert.Equal(expectedType, result.Type);

        Assert.IsType<TpWeldInstructionRegister>(result.Args);
        var register = (TpWeldInstructionRegister)result.Args;

        Assert.IsAssignableFrom<TpRegister>(register.Register);
        var directAccess = register.Register;
        Assert.IsType<TpAccessDirect>(directAccess.Access);
        Assert.Equal(expectedRegisterNumber, ((TpAccessDirect)directAccess.Access).Number);
    }

    [Theory]
    [InlineData("Arc Start[R[AR[1]]]", TpArcWeldingOptionType.Start, 1)]
    [InlineData("Arc Start[R[R[1]]]", TpArcWeldingOptionType.Start, 1)]
    public void ParseWeldInstruction_WithRegisterScheduleIndirect_ParsesCorrectly(
        string input,
        TpArcWeldingOptionType expectedType,
        int expectedRegisterNumber)
    {
        var result = TpWeldInstruction.GetParser().Parse(input) as TpWeldInstruction;

        Assert.NotNull(result);
        Assert.Equal(expectedType, result.Type);

        Assert.IsType<TpWeldInstructionRegister>(result.Args);
        var register = (TpWeldInstructionRegister)result.Args;

        Assert.IsAssignableFrom<TpRegister>(register.Register);
        var indirectAccessRegister = register.Register;
        Assert.IsType<TpAccessIndirect>(indirectAccessRegister.Access);
        var indirectAccess = indirectAccessRegister.Access as TpAccessIndirect;
        Assert.IsType<TpAccessDirect>(indirectAccess?.Register.Access);
        var directAccess = indirectAccess.Register.Access as TpAccessDirect;

        Assert.Equal(expectedRegisterNumber, directAccess?.Number);
    }

    [Theory]
    [InlineData("Arc Start[1.5,2.0,3.2]", TpArcWeldingOptionType.Start, 3)]
    [InlineData("Arc End[0.5,1.0,1.5,2.0]", TpArcWeldingOptionType.End, 4)]
    [InlineData("Arc End[0,0.0IPM,0.000,0.00,0.0s]", TpArcWeldingOptionType.End, 5)]
    public void ParseWeldInstruction_WithParameters_ParsesCorrectly(
        string input,
        TpArcWeldingOptionType expectedType,
        int expectedParameterCount)
    {
        var result = TpWeldInstruction.GetParser().Parse(input) as TpWeldInstruction;

        Assert.NotNull(result);
        Assert.Equal(expectedType, result.Type);

        Assert.IsType<TpWeldInstructionParameters>(result.Args);
        var parameters = (TpWeldInstructionParameters)result.Args;

        Assert.NotNull(parameters.Parameters);
        Assert.Equal(expectedParameterCount, parameters.Parameters.Count);
    }

    [Theory]
    [InlineData("Arc[1]")]
    [InlineData("Arc Middle[5]")]
    [InlineData("Weld Start[1]")]
    [InlineData("ARC start[5]")]
    public void ParseWeldInstruction_InvalidFormat_ThrowsParseException(string input) =>
        Assert.Throws<ParseException>(() => TpWeldInstruction.GetParser().Parse(input));
}
