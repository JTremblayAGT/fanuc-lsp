using AGT.TPLangParser.TPLang;
using AGT.TPLangParser.TPLang.Instructions;
using Sprache;

namespace AGT.TPLangParser.Tests;

public class TpMathInstructionTests
{
    [Theory]
    [InlineData("R[1]=SQRT[R[2]]")]
    [InlineData("R[10]=SQRT[R[20]]")]
    [InlineData("AR[1]=SQRT[R[5]]")]
    public void Parse_SqrtExpression_ParsesCorrectly(string input)
    {
        var result = TpMathInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMathInstruction>(result);

        Assert.NotNull(result.Variable);
        Assert.NotNull(result.Expression);
        Assert.IsType<TpSqrtExpression>(result.Expression);
    }

    [Theory]
    [InlineData("R[1]=SIN[R[2]]", typeof(TpSinExpression))]
    [InlineData("R[1]=COS[R[2]]", typeof(TpCosExpression))]
    [InlineData("R[1]=TAN[R[2]]", typeof(TpTanExpression))]
    [InlineData("R[1]=ASIN[R[2]]", typeof(TpAsinExpression))]
    [InlineData("R[1]=ACOS[R[2]]", typeof(TpAcosExpression))]
    [InlineData("R[1]=ATAN[R[2]]", typeof(TpAtanExpression))]
    public void Parse_TrigonometricExpression_ParsesCorrectly(string input, Type expectedType)
    {
        var result = TpMathInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMathInstruction>(result);

        Assert.NotNull(result.Variable);
        Assert.NotNull(result.Expression);
        Assert.IsType(expectedType, result.Expression);
    }

    [Theory]
    [InlineData("R[1]=ABS[R[2]]")]
    [InlineData("R[10]=ABS[R[20]]")]
    [InlineData("AR[1]=ABS[R[5]]")]
    public void Parse_AbsoluteExpression_ParsesCorrectly(string input)
    {
        var result = TpMathInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMathInstruction>(result);

        Assert.NotNull(result.Variable);
        Assert.NotNull(result.Expression);
        Assert.IsType<TpAbsExpression>(result.Expression);
    }

    [Theory]
    [InlineData("R[1]=SQRT[R[-1]]")] // Negative register number
    [InlineData("R[1]=SQRT[]")] // Missing register
    [InlineData("R[1]=SQRT[A]")] // Invalid register
    [InlineData("R[1]=SQRT")] // Missing brackets
    [InlineData("R[1]=SIN")] // Missing brackets
    [InlineData("R[1]=COS")] // Missing brackets
    [InlineData("R[1]=TAN")] // Missing brackets
    [InlineData("R[1]=ABS")] // Missing brackets
    public void Parse_InvalidMathExpression_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpMathInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("R[1] = SQRT[R[2]]")] // Spaces around equals
    [InlineData("R[1]=  SQRT[R[2]]")] // Extra spaces before function
    [InlineData("R[1]=SQRT  [R[2]]")] // Spaces before opening bracket
    [InlineData("R[1]=SQRT[  R[2]  ]")] // Spaces inside brackets
    public void Parse_MathExpression_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpMathInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("r[1]=SQRT[R[2]]")] // Lowercase r
    [InlineData("R[1]=sqrt[R[2]]")] // Lowercase function
    [InlineData("R[1]=Sqrt[R[2]]")] // Mixed case function
    [InlineData("R[1]=SIN[r[2]]")] // Lowercase inner register
    public void Parse_MathExpression_CaseInsensitive_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpMathInstruction.GetParser().Parse(input));
}
