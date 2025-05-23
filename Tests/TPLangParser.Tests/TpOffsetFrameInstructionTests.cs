using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;
using Sprache;

namespace TPLangParser.Tests;

public class TpOffsetFrameInstructionTests
{
    [Theory]
    [InlineData("OFFSET CONDITION PR[1]", 1)]
    [InlineData("OFFSET CONDITION PR[5], UFRAME[2]", 5)]
    [InlineData("OFFSET CONDITION PR[10]", 10)]
    public void Parse_OffsetCondition_ParsesCorrectly(string input, int expectedPrNumber)
    {
        var result = TpOffsetFrameInstruction.GetParser().Parse(input);

        var instruction = Assert.IsType<TpOffsetConditionInstruction>(result);
        var access = Assert.IsType<TpAccessDirect>(instruction.PositionRegister.Access);
        Assert.Equal(expectedPrNumber, access.Number);
    }

    [Theory]
    [InlineData("UFRAME_NUM=1", 1, 1)]
    [InlineData("UFRAME_NUM[GP1]=5", 1, 5)]
    [InlineData("UFRAME_NUM[GP2]=R[1]", 2, typeof(TpValueRegister))]
    public void Parse_UserFrameUse_ParsesCorrectly(string input, int expectedGroup, object expectedValue)
    {
        var result = TpOffsetFrameInstruction.GetParser().Parse(input);

        var instruction = Assert.IsType<TpUserFrameUseInstruction>(result);
        Assert.Equal(expectedGroup, instruction.GroupNumber);

        switch (expectedValue)
        {
            case int intValue:
            {
                var value = Assert.IsType<TpValueIntegerConstant>(instruction.Value);
                Assert.Equal(intValue, value.Value);
                break;
            }
            case Type type:
                Assert.IsType(type, instruction.Value);
                break;
        }
    }

    [Theory]
    [InlineData("UTOOL_NUM=1", 1, 1)]
    [InlineData("UTOOL_NUM[GP1]=5", 1, 5)]
    [InlineData("UTOOL_NUM[GP2]=R[1]", 2, typeof(TpValueRegister))]
    public void Parse_UserToolUse_ParsesCorrectly(string input, int expectedGroup, object expectedValue)
    {
        var result = TpOffsetFrameInstruction.GetParser().Parse(input);

        var instruction = Assert.IsType<TpUserToolUseInstruction>(result);
        Assert.Equal(expectedGroup, instruction.GroupNumber);

        switch (expectedValue)
        {
            case int intValue:
            {
                var value = Assert.IsType<TpValueIntegerConstant>(instruction.Value);
                Assert.Equal(intValue, value.Value);
                break;
            }
            case Type type:
                Assert.IsType(type, instruction.Value);
                break;
        }
    }

    [Theory]
    [InlineData("UFRAME[1]=PR[1]", 1, 1)]
    [InlineData("UFRAME[5]=PR[10]", 5, 10)]
    public void Parse_UserFrameSet_ParsesCorrectly(string input, int expectedFrameNumber, int expectedPrNumber)
    {
        var result = TpOffsetFrameInstruction.GetParser().Parse(input);

        var instruction = Assert.IsType<TpUserFrameSetInstruction>(result);

        var frameAccess = Assert.IsType<TpAccessDirect>(instruction.UserFrame.Access);
        Assert.Equal(expectedFrameNumber, frameAccess.Number);

        var prAccess = Assert.IsType<TpAccessDirect>(instruction.Value.Access);
        Assert.Equal(expectedPrNumber, prAccess.Number);
    }

    [Theory]
    [InlineData("UTOOL[1]=PR[1]", 1, 1)]
    [InlineData("UTOOL[5]=PR[10]", 5, 10)]
    public void Parse_UserToolSet_ParsesCorrectly(string input, int expectedToolNumber, int expectedPrNumber)
    {
        var result = TpOffsetFrameInstruction.GetParser().Parse(input);

        var instruction = Assert.IsType<TpUserToolSetInstruction>(result);

        var toolAccess = Assert.IsType<TpAccessDirect>(instruction.UserTool.Access);
        Assert.Equal(expectedToolNumber, toolAccess.Number);

        var prAccess = Assert.IsType<TpAccessDirect>(instruction.Value.Access);
        Assert.Equal(expectedPrNumber, prAccess.Number);
    }

    [Theory]
    [InlineData("OFFSET CONDITION")] // Missing PR
    [InlineData("OFFSET CONDITION PR")] // Incomplete PR
    [InlineData("UFRAME_NUM[GP]=1")] // Missing GP number
    [InlineData("UFRAME_NUM[GP1]")] // Missing value
    [InlineData("UTOOL_NUM=")] // Missing value
    [InlineData("UFRAME[]")] // Missing frame number
    [InlineData("UTOOL[1]")] // Missing PR assignment
    public void Parse_InvalidOffsetFrameInstruction_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpOffsetFrameInstruction.GetParser().Parse(input));
}