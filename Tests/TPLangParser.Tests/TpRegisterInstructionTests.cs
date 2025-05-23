using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;
using Sprache;

namespace TPLangParser.Tests;
public class TpRegisterInstructionTests
{
    [Theory]
    [InlineData("R[1]=5")]
    [InlineData("R[10]=100")]
    [InlineData("R[25]=0")]
    public void Parse_RegisterAssignment_SimpleConstant_ParsesCorrectly(string input)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var assignment = Assert.IsType<TpRegisterAssignment>(result);
        Assert.NotNull(assignment.Register);
        var value = Assert.IsType<TpArithmeticValue>(assignment.Expression);
        Assert.IsType<TpValueIntegerConstant>(value.Value);
    }

    [Theory]
    [InlineData("R[1]=5.5")]
    [InlineData("R[10]=0.1")]
    [InlineData("R[25]=(-3.14)")]
    public void Parse_RegisterAssignment_FloatingPointConstant_ParsesCorrectly(string input)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var assignment = Assert.IsType<TpRegisterAssignment>(result);
        var value = Assert.IsType<TpArithmeticValue>(assignment.Expression);
        Assert.IsType<TpValueFloatingPointConstant>(value.Value);
    }

    [Theory]
    [InlineData("R[1]=R[2]")]
    [InlineData("R[10]=AR[5]")]
    public void Parse_RegisterAssignment_RegisterValue_ParsesCorrectly(string input)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var assignment = Assert.IsType<TpRegisterAssignment>(result);
        var value = Assert.IsType<TpArithmeticValue>(assignment.Expression);
        Assert.IsType<TpValueRegister>(value.Value);
    }

    [Theory]
    [InlineData("R[1]=DI[1]")]
    [InlineData("R[10]=DO[5]")]
    [InlineData("R[25]=RO[3]")]
    public void Parse_RegisterAssignment_IOPortValue_ParsesCorrectly(string input)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var assignment = Assert.IsType<TpRegisterAssignment>(result);
        var value = Assert.IsType<TpArithmeticValue>(assignment.Expression);
        Assert.IsType<TpValueIOPort>(value.Value);
    }

    [Theory]
    [InlineData("R[1]=F[1]")]
    [InlineData("R[10]=F[5]")]
    public void Parse_RegisterAssignment_FlagValue_ParsesCorrectly(string input)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var assignment = Assert.IsType<TpRegisterAssignment>(result);
        var value = Assert.IsType<TpArithmeticValue>(assignment.Expression);
        Assert.IsType<TpValueFlag>(value.Value);
    }

    [Theory]
    [InlineData("R[1]=$TIMER[1]")]
    [InlineData("R[10]=$GROUP[1].$CURRENT_ANG[2]")]
    public void Parse_RegisterAssignment_SystemVariable_ParsesCorrectly(string input)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var assignment = Assert.IsType<TpRegisterAssignment>(result);
        var value = Assert.IsType<TpArithmeticValue>(assignment.Expression);
        Assert.IsType<TpValueSystemVariable>(value.Value);
    }

    [Theory]
    [InlineData("R[1]=SQRT[R[2]]")]
    [InlineData("R[10]=SIN[R[5]]")]
    [InlineData("R[25]=COS[R[3]]")]
    public void Parse_RegisterAssignment_MathExpression_ParsesCorrectly(string input)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var assignment = Assert.IsType<TpRegisterAssignment>(result);
        var value = Assert.IsType<TpArithmeticValue>(assignment.Expression);
        Assert.IsType<TpValueMathExpr>(value.Value);
    }

    [Theory]
    [InlineData("R[1]=R[2]+5")]
    [InlineData("R[10]=10+R[5]")]
    [InlineData("R[25]=R[1]+R[2]")]
    public void Parse_RegisterAssignment_Addition_ParsesCorrectly(string input)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpRegisterAssignment>(result);
        Assert.IsType<TpArithmeticAddition>(assignment.Expression);
    }

    [Theory]
    [InlineData("R[1]=R[2]-5")]
    [InlineData("R[10]=10-R[5]")]
    [InlineData("R[25]=R[1]-R[2]")]
    [InlineData("R[34:MathTemp]=R[271:TargetZone]-R[270:StartZone]")]
    public void Parse_RegisterAssignment_Subtraction_ParsesCorrectly(string input)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpRegisterAssignment>(result);
        Assert.IsType<TpArithmeticSubtraction>(assignment.Expression);
    }

    [Theory]
    [InlineData("R[1]=R[2]*5")]
    [InlineData("R[10]=10/R[5]")]
    [InlineData("R[25]=R[1]DIV5")]
    [InlineData("R[30]=R[3]MOD2")]
    public void Parse_RegisterAssignment_OtherOperations_ParsesCorrectly(string input)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpRegisterAssignment>(result);
        Assert.IsAssignableFrom<TpArithmeticBinary>(assignment.Expression);
    }

    [Theory]
    [InlineData("R[1]=R[2]+5*2")]
    [InlineData("R[10]=10+R[5]/5")]
    [InlineData("R[25]=R[1]*R[2]+R[3]")]
    public void Parse_RegisterAssignment_ComplexExpression_ParsesCorrectly(string input)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpRegisterAssignment>(result);
        Assert.NotNull(assignment.Expression);
        Assert.IsAssignableFrom<TpArithmeticBinary>(assignment.Expression);
    }

    [Theory]
    [InlineData("R[1]=")] // Missing expression
    [InlineData("R=5")] // Invalid R format
    [InlineData("R[]=")] // Missing register number
    public void Parse_InvalidRegisterInstruction_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpRegisterInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("R[1] = 5")]  // Spaces around equals
    [InlineData("R[ 1 ] = 10")]  // Spaces inside brackets
    [InlineData("R[1] = R[2] + 5")]  // Spaces around operator
    [InlineData("R[1] = SQRT[ R[2] ]")]  // Spaces inside math function
    public void Parse_RegisterInstruction_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("r[1]=5")]  // Lowercase r
    [InlineData("R[1]=sqrt(R[2])")]  // Lowercase function name
    public void Parse_RegisterInstruction_CaseSensitive_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpRegisterInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("R[GP1:1]=5", 1, 1)]
    [InlineData("R[GP2:5]=10", 2, 5)]
    public void Parse_RegisterAssignment_WithGroup_ParsesCorrectly(string input, int expectedGroup, int expectedNumber)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var assignment = Assert.IsType<TpRegisterAssignment>(result);
        var access = Assert.IsType<TpAccessDirect>(assignment.Register.Access);
        Assert.Equal(expectedNumber, access.Number);
        Assert.Equal(expectedGroup, access.Group);
    }

    [Theory]
    [InlineData("R[1:Comment]=5", 1, "Comment")]
    [InlineData("R[5:Test Register]=10", 5, "Test Register")]
    public void Parse_RegisterAssignment_WithComment_ParsesCorrectly(string input, int expectedNumber, string expectedComment)
    {
        var result = TpRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var assignment = Assert.IsType<TpRegisterAssignment>(result);
        var access = Assert.IsType<TpAccessDirect>(assignment.Register.Access);
        Assert.Equal(expectedNumber, access.Number);
        Assert.Equal(expectedComment, access.Comment);
    }
}
