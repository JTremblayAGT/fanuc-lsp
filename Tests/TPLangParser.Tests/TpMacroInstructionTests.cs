using TPLangParser.TPLang.Instructions;
using Sprache;

namespace TPLangParser.Tests;

public class TpMacroInstructionTests
{
    [Theory]
    [InlineData("MACRO_1", "MACRO_1")]
    [InlineData("TEST", "TEST")]
    [InlineData("MY_MACRO", "MY_MACRO")]
    [InlineData("MACRO_ROUTINE_1", "MACRO_ROUTINE_1")]
    public void Parse_MacroInstruction_ParsesCorrectly(string input, string expectedProgramName)
    {
        var result = TpMacroInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMacroInstruction>(result);

        Assert.Equal(expectedProgramName, result.ProgramName);
    }

    [Theory]
    [InlineData("TEST  ")] // Trailing spaces
    [InlineData("  TEST")] // Leading spaces
    [InlineData("\tTEST")] // Tab
    [InlineData("TEST\t")] // Trailing tab
    public void Parse_MacroInstruction_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpMacroInstruction.GetParser().Parse(input);
        Assert.NotNull(result);

        Assert.Equal(input.Trim(), result.ProgramName);
    }

    [Theory]
    [InlineData("A")] // Single character
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ")] // All letters
    [InlineData("A_1_2_3")] // Letters, numbers, and underscores
    [InlineData("TEST_123_ABC")] // Mixed pattern
    public void Parse_MacroInstruction_ValidProgramNamePatterns_ParsesCorrectly(string input)
    {
        var result = TpMacroInstruction.GetParser().Parse(input);
        Assert.NotNull(result);

        Assert.Equal(input, result.ProgramName);
    }

    [Theory]
    [InlineData("MACRO_WITH_VERY_LONG_NAME_THAT_MIGHT_EXCEED_NORMAL_LIMITS")] // Very long name
    [InlineData("M")] // Single character
    [InlineData("A1")] // Two characters
    public void Parse_MacroInstruction_NameLengthEdgeCases_ParsesCorrectly(string input)
    {
        var result = TpMacroInstruction.GetParser().Parse(input);
        Assert.NotNull(result);

        Assert.Equal(input, result.ProgramName);
    }

    [Fact]
    public void Parse_MacroInstruction_PreservesOriginalProgramName()
    {
        const string input = "TEST_MACRO_123";
        var result = TpMacroInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.Equal(input, result.ProgramName);
        Assert.Equal(input.GetHashCode(), result.ProgramName.GetHashCode());
    }
}
