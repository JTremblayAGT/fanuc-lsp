using AGT.TPLangParser.TPLang.Instructions;
using Sprache;

namespace AGT.TPLangParser.Tests;

public class TpConditionMonitorInstructionTests
{
    [Theory]
    [InlineData("MONITOR TEST", "TEST")]
    [InlineData("MONITOR PROGRAM_1", "PROGRAM_1")]
    [InlineData("MONITOR MY_ROUTINE", "MY_ROUTINE")]
    public void Parse_MonitorInstruction_ParsesCorrectly(string input, string expectedProgramName)
    {
        var result = TpConditionMonitorInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMonitorInstruction>(result);

        var monitor = (TpMonitorInstruction)result;
        Assert.Equal(expectedProgramName, monitor.ProgramName);
    }

    [Theory]
    [InlineData("MONITOR END TEST", "TEST")]
    [InlineData("MONITOR END PROGRAM_1", "PROGRAM_1")]
    [InlineData("MONITOR END MY_ROUTINE", "MY_ROUTINE")]
    public void Parse_MonitorEndInstruction_ParsesCorrectly(string input, string expectedProgramName)
    {
        var result = TpConditionMonitorInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMonitorEndInstruction>(result);

        var monitorEnd = (TpMonitorEndInstruction)result;
        Assert.Equal(expectedProgramName, monitorEnd.ProgramName);
    }

    [Theory]
    [InlineData("MONITOR  TEST")] // Extra space
    [InlineData("MONITOR\tTEST")] // Tab
    [InlineData("MONITOR END  TEST")] // Extra space in END
    [InlineData("MONITOR END\tTEST")] // Tab in END
    public void Parse_MonitorInstruction_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpConditionMonitorInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("monitor TEST")] // lowercase keyword
    [InlineData("Monitor TEST")] // Mixed case keyword
    [InlineData("monitor end TEST")] // all lowercase
    [InlineData("Monitor End TEST")] // Title case
    public void Parse_MonitorInstruction_CaseInsensitive_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpConditionMonitorInstruction.GetParser().Parse(input));

    [Fact]
    public void Parse_WhenInstruction_ThrowsParseException()
    {
        // WHEN instruction is not implemented according to the TODO comment
        const string input = "WHEN R[1]=1 DO TEST";
        Assert.Throws<ParseException>(() => TpConditionMonitorInstruction.GetParser().Parse(input));
    }
}
