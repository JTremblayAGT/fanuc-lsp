using AGT.TPLangParser.TPLang;
using AGT.TPLangParser.TPLang.Instructions;
using Sprache;

namespace AGT.TPLangParser.Tests;

public class TpPayloadInstructionTests
{
    [Theory]
    [InlineData("PAYLOAD[1]", 1)]
    [InlineData("PAYLOAD[10]", 10)]
    [InlineData("PAYLOAD[100]", 100)]
    public void Parse_PayloadInstruction_ParsesCorrectly(string input, int expectedNumber)
    {
        var result = TpPayloadInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpPayloadInstruction>(result);

        var access = Assert.IsType<TpAccessDirect>(result.Access);
        Assert.Equal(expectedNumber, access.Number);
    }

    [Theory]
    [InlineData("PAYLOAD[GP1:1]", 1, 1)]
    [InlineData("PAYLOAD[GP2:5]", 2, 5)]
    public void Parse_PayloadInstruction_WithGroup_ParsesCorrectly(string input, int expectedGroup, int expectedNumber)
    {
        var result = TpPayloadInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var access = Assert.IsType<TpAccessDirect>(result.Access);
        Assert.Equal(expectedNumber, access.Number);
        Assert.Equal(expectedGroup, access.Group);
    }

    [Theory]
    [InlineData("PAYLOAD[1:Comment]", 1, "Comment")]
    [InlineData("PAYLOAD[5:Test Payload]", 5, "Test Payload")]
    public void Parse_PayloadInstruction_WithComment_ParsesCorrectly(string input, int expectedNumber, string expectedComment)
    {
        var result = TpPayloadInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var access = Assert.IsType<TpAccessDirect>(result.Access);
        Assert.Equal(expectedNumber, access.Number);
        Assert.Equal(expectedComment, access.Comment);
    }

    [Theory]
    [InlineData("PAYLOAD")]  // Missing brackets
    [InlineData("PAYLOAD[]")]  // Missing number
    [InlineData("PAYLOAD[GP]")]  // Incomplete group
    [InlineData("PAYLOAD[GP:]")]  // Missing group number
    [InlineData("PAYLOAD[GP1:]")]  // Missing payload number
    public void Parse_InvalidPayloadInstruction_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpPayloadInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("PAYLOAD  [1]")]  // Extra space before bracket
    [InlineData("PAYLOAD[  1  ]")]  // Spaces around number
    [InlineData("PAYLOAD[GP1  :  1]")]  // Spaces around colon
    [InlineData("PAYLOAD[1  :Comment]")]  // Space before comment
    public void Parse_PayloadInstruction_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpPayloadInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("payload[1]")]  // Lowercase keyword
    [InlineData("PAYLOAD[gp1:1]")]  // Lowercase GP
    public void Parse_PayloadInstruction_CaseInsensitive_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpPayloadInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("PAYLOAD[GP1:1:Comment]", 1, 1, "Comment")]
    [InlineData("PAYLOAD[GP2:5:Test Load]", 2, 5, "Test Load")]
    public void Parse_PayloadInstruction_WithGroupAndComment_ParsesCorrectly(
        string input, int expectedGroup, int expectedNumber, string expectedComment)
    {
        var result = TpPayloadInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var access = Assert.IsType<TpAccessDirect>(result.Access);
        Assert.Equal(expectedNumber, access.Number);
        Assert.Equal(expectedGroup, access.Group);
        Assert.Equal(expectedComment, access.Comment);
    }
}
