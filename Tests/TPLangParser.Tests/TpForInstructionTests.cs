using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;
using Sprache;

namespace TPLangParser.Tests;

public class TpForInstructionTests
{
    [Theory]
    [InlineData("FOR R[1]=1 TO 10", 1, 1, 10, TpForCountDirection.Up)]
    [InlineData("FOR R[10]=5 TO 100", 10, 5, 100, TpForCountDirection.Up)]
    [InlineData("FOR R[1]=100 DOWNTO 1", 1, 100, 1, TpForCountDirection.Down)]
    public void Parse_BeginForInstruction_ValuesAndDirection_ParsesCorrectly(
        string input,
        int expectedCounterRegister,
        int expectedInitialValue,
        int expectedTargetValue,
        TpForCountDirection expectedDirection)
    {
        var result = TpForInstruction.GetParser().Parse(input);
        var forInst = (TpBeginForInstruction)result;

        Assert.Equal(expectedCounterRegister, ((TpAccessDirect)forInst.Counter.Access).Number);

        if (forInst.InitialValue is TpValueIntegerConstant initValue)
        {
            Assert.Equal(expectedInitialValue, initValue.Value);
        }

        if (forInst.TargetValue is TpValueIntegerConstant targetValue)
        {
            Assert.Equal(expectedTargetValue, targetValue.Value);
        }

        Assert.Equal(expectedDirection, forInst.CountDirection);
    }

    [Theory]
    [InlineData("FOR R[1]=R[2] TO R[3]", 1, 2, 3)]
    [InlineData("FOR R[10]=R[20] DOWNTO R[30]", 10, 20, 30)]
    public void Parse_BeginForInstruction_WithRegisterValues_ParsesCorrectly(
        string input,
        int expectedCounterRegister,
        int expectedInitialRegister,
        int expectedTargetRegister)
    {
        var result = TpForInstruction.GetParser().Parse(input);
        var forInst = (TpBeginForInstruction)result;

        Assert.Equal(expectedCounterRegister, ((TpAccessDirect)forInst.Counter.Access).Number);

        var initialReg = Assert.IsType<TpValueRegister>(forInst.InitialValue);
        Assert.Equal(expectedInitialRegister, ((TpAccessDirect)initialReg.Register.Access).Number);

        var targetReg = Assert.IsType<TpValueRegister>(forInst.TargetValue);
        Assert.Equal(expectedTargetRegister, ((TpAccessDirect)targetReg.Register.Access).Number);
    }

    [Fact]
    public void Parse_EndForInstruction_ParsesCorrectly()
    {
        const string input = "ENDFOR";
        var result = TpForInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpEndForInstruction>(result);
    }

    [Theory]
    [InlineData("FOR")] // Missing counter
    [InlineData("FOR R[1]")] // Missing assignment
    [InlineData("FOR R[1]=")] // Missing initial value
    [InlineData("FOR R[1]=1")] // Missing direction and target
    [InlineData("FOR R[1]=1 TO")] // Missing target value
    [InlineData("FOR R[1]=1 DOWNTO")] // Missing target value
    [InlineData("FOR R[1]=A TO 10")] // Invalid initial value
    [InlineData("FOR R[1]=1 TO B")] // Invalid target value
    [InlineData("FOR R[1]=1.5 TO 10")] // Non-integer value
    [InlineData("FOR R[1]=1 UP 10")] // Invalid direction
    public void Parse_InvalidForInstruction_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpForInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("FOR  R[1]=1 TO 10")] // Extra space after FOR
    [InlineData("FOR R[1] = 1 TO 10")] // Spaces around equals
    [InlineData("FOR R[1]=1  TO  10")] // Extra spaces around TO
    [InlineData("FOR\tR[1]=1\tTO\t10")] // Using tabs
    public void Parse_ForInstruction_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpForInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpBeginForInstruction>(result);
    }

    [Theory]
    [InlineData("for R[1]=1 TO 10")] // lowercase for
    [InlineData("For R[1]=1 TO 10")] // Title case for
    [InlineData("FOR R[1]=1 to 10")] // lowercase to
    [InlineData("FOR R[1]=1 downto 10")] // lowercase downto
    [InlineData("endfor")] // lowercase endfor
    [InlineData("EndFor")] // Mixed case endfor
    public void Parse_ForInstruction_CaseInsensitive_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpForInstruction.GetParser().Parse(input));
}
