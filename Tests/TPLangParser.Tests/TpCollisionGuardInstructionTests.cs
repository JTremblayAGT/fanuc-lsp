using AGT.TPLangParser.TPLang;
using AGT.TPLangParser.TPLang.Instructions;
using Sprache;

namespace AGT.TPLangParser.Tests;

public class TpCollisionGuardInstructionTests
{
    [Theory]
    [InlineData("COL DETECT")]
    [InlineData("COL")]
    [InlineData("DETECT ON")]
    [InlineData("COL DETECT ENABLED")]
    [InlineData("COL DETECT YES")]
    public void Parse_InvalidCollisionDetect_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpCollisionGuardInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("COL DETECT ON", TpOnOffState.On)]
    [InlineData("COL DETECT OFF", TpOnOffState.Off)]
    public void Parse_CollisionDetect_State_ParsesCorrectly(string input, TpOnOffState expectedState)
    {
        var result = TpCollisionGuardInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpCollisionDetectInstruction>(result);

        var colDetect = (TpCollisionDetectInstruction)result;
        Assert.Equal(expectedState, colDetect.State);
    }

    [Theory]
    [InlineData("COL DETECT ON")] // Whitespace after ON
    [InlineData("COL DETECT OFF ")] // Whitespace after OFF
    [InlineData("COL DETECT\tON")] // Tab before ON
    public void Parse_CollisionDetect_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpCollisionGuardInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpCollisionDetectInstruction>(result);
    }

    [Theory]
    [InlineData("col detect on")] // lowercase
    [InlineData("Col Detect On")] // Mixed case
    [InlineData("COL DETECT on")] // Mixed case state
    [InlineData("COL  DETECT  ON")] // Extra whitespace between words, 'COL DETECT' is a keyword
    public void Parse_CollisionDetect_CaseInsensitive_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpCollisionGuardInstruction.GetParser().Parse(input));
}
