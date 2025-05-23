using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;
using Sprache;

namespace TPLangParser.Tests;
public class TpWaitInstructionTests
{
    [Theory]
    [InlineData("WAIT 5")]
    [InlineData("WAIT 0.5")]
    [InlineData("WAIT 10")]
    public void Parse_WaitTime_Constant_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitTime = Assert.IsType<TpWaitTime>(result);
        Assert.NotNull(waitTime.WaitTime);
    }

    [Theory]
    [InlineData("WAIT 5", 5)]
    [InlineData("WAIT 10", 10)]
    public void Parse_WaitTime_IntegerConstant_VerifyValue(string input, int expectedValue)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        var waitTime = Assert.IsType<TpWaitTime>(result);
        var value = Assert.IsType<TpValueIntegerConstant>(waitTime.WaitTime);
        Assert.Equal(expectedValue, value.Value);
    }

    [Theory]
    [InlineData("WAIT 5.5", 5.5)]
    [InlineData("WAIT 0.1", 0.1)]
    public void Parse_WaitTime_FloatingPointConstant_VerifyValue(string input, double expectedValue)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        var waitTime = Assert.IsType<TpWaitTime>(result);
        var value = Assert.IsType<TpValueFloatingPointConstant>(waitTime.WaitTime);
        Assert.Equal(expectedValue, value.Value);
    }

    [Theory]
    [InlineData("WAIT R[1]")]
    [InlineData("WAIT AR[5]")]
    public void Parse_WaitTime_Register_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitTime = Assert.IsType<TpWaitTime>(result);
        var value = Assert.IsType<TpValueRegister>(waitTime.WaitTime);
        Assert.NotNull(value.Register);
    }

    [Theory]
    [InlineData("WAIT R[1]", 1)]
    [InlineData("WAIT R[10]", 10)]
    public void Parse_WaitTime_Register_VerifyRegisterNumber(string input, int expectedNumber)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        var waitTime = Assert.IsType<TpWaitTime>(result);
        var value = Assert.IsType<TpValueRegister>(waitTime.WaitTime);
        var access = Assert.IsType<TpAccessDirect>(value.Register.Access);
        Assert.Equal(expectedNumber, access.Number);
    }

    [Theory]
    [InlineData("WAIT $TIMER[1]")]
    [InlineData("WAIT $GV[1]")]
    public void Parse_WaitTime_SystemVariable_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitTime = Assert.IsType<TpWaitTime>(result);
        Assert.IsType<TpValueSystemVariable>(waitTime.WaitTime);
    }

    [Theory]
    [InlineData("WAIT R[1]=1")]
    [InlineData("WAIT R[10]=100")]
    [InlineData("WAIT R[25]=0")]
    public void Parse_WaitCondition_SimpleRegisterComparison_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionSingle>(waitCondition.Condition);
        var comparison = Assert.IsType<TpRegisterComparisonExpression>(logicExpr.Expression);
        Assert.Equal(TpComparisonOperator.Equal, comparison.Operator);
        Assert.Null(waitCondition.TimeoutLabel);
    }

    [Theory]
    [InlineData("WAIT R[1]<>1")]
    [InlineData("WAIT R[10]>100")]
    [InlineData("WAIT R[25]<0")]
    [InlineData("WAIT R[5]>=10")]
    [InlineData("WAIT R[15]<=20")]
    public void Parse_WaitCondition_DifferentOperators_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionSingle>(waitCondition.Condition);
        var comparison = Assert.IsType<TpRegisterComparisonExpression>(logicExpr.Expression);
        Assert.NotEqual(TpComparisonOperator.Equal, comparison.Operator);
    }

    [Theory]
    [InlineData("WAIT R[1]=R[2]")]
    [InlineData("WAIT R[10]=AR[5]")]
    public void Parse_WaitCondition_RegisterComparison_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionSingle>(waitCondition.Condition);
        var comparison = Assert.IsType<TpRegisterComparisonExpression>(logicExpr.Expression);
        Assert.IsType<TpValueRegister>(comparison.Rhs);
    }

    [Theory]
    [InlineData("WAIT DI[1]=ON")]
    [InlineData("WAIT DO[5]=OFF")]
    [InlineData("WAIT RI[3]=ON")]
    public void Parse_WaitCondition_DigitalIOComparison_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionSingle>(waitCondition.Condition);
        var comparison = Assert.IsType<TpDigitalIOComparisonExpression>(logicExpr.Expression);
        Assert.IsType<TpValueIOState>(comparison.Rhs);
    }

    [Theory]
    [InlineData("WAIT AI[1]=5")]
    [InlineData("WAIT AO[5]>10")]
    [InlineData("WAIT GI[3]<100")]
    public void Parse_WaitCondition_AnalogIOComparison_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionSingle>(waitCondition.Condition);
        var comparison = Assert.IsType<TpAnalogIOComparisonExpression>(logicExpr.Expression);
        Assert.IsType<TpValueIntegerConstant>(comparison.Rhs);
    }

    [Theory]
    [InlineData("WAIT $GROUP[1].$CURRENT_ANG[2]=0")]
    [InlineData("WAIT $TIMER[1]>10")]
    public void Parse_WaitCondition_ParameterComparison_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionSingle>(waitCondition.Condition);
        var comparison = Assert.IsType<TpParameterComparisonExpression>(logicExpr.Expression);
    }

    [Theory]
    [InlineData("WAIT R[1]=1 AND R[2]=2")]
    [InlineData("WAIT DI[1]=ON AND DO[2]=OFF")]
    [InlineData("WAIT R[1]>10 AND AI[1]<50")]
    public void Parse_WaitCondition_AndCondition_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionAnd>(waitCondition.Condition);
        Assert.Equal(2, logicExpr.Expression.Count);
    }

    [Theory]
    [InlineData("WAIT R[1]=1 OR R[2]=2")]
    [InlineData("WAIT DI[1]=ON OR DO[2]=OFF")]
    [InlineData("WAIT R[1]>10 OR AI[1]<50")]
    public void Parse_WaitCondition_OrCondition_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionOr>(waitCondition.Condition);
        Assert.Equal(2, logicExpr.Expression.Count);
    }

    [Theory]
    [InlineData("WAIT R[1]=1 AND R[2]=2 AND R[3]=3")]
    [InlineData("WAIT DI[1]=ON AND DO[2]=OFF AND RI[3]=ON")]
    public void Parse_WaitCondition_MultipleAndConditions_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionAnd>(waitCondition.Condition);
        Assert.Equal(3, logicExpr.Expression.Count);
    }

    [Theory]
    [InlineData("WAIT R[1]=1 OR R[2]=2 OR R[3]=3")]
    [InlineData("WAIT DI[1]=ON OR DO[2]=OFF OR RI[3]=ON")]
    public void Parse_WaitCondition_MultipleOrConditions_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionOr>(waitCondition.Condition);
        Assert.Equal(3, logicExpr.Expression.Count);
    }

    [Theory]
    [InlineData("WAIT R[1]=1 TIMEOUT, LBL[1]")]
    [InlineData("WAIT DI[1]=ON TIMEOUT, LBL[10]")]
    public void Parse_WaitCondition_WithTimeoutLabel_ParsesCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        Assert.NotNull(waitCondition.TimeoutLabel);
    }

    [Theory]
    [InlineData("WAIT R[1]=1 TIMEOUT, LBL[1]", 1)]
    [InlineData("WAIT DI[1]=ON TIMEOUT, LBL[10]", 10)]
    public void Parse_WaitCondition_VerifyTimeoutLabelNumber(string input, int expectedLabelNumber)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        var label = waitCondition.TimeoutLabel;
        var access = Assert.IsType<TpAccessDirect>(label!.LabelNumber);
        Assert.Equal(expectedLabelNumber, access.Number);
    }

    [Theory]
    [InlineData("WAIT R[1]=1 TIMEOUT, LBL[1:Error]", 1, "Error")]
    [InlineData("WAIT DI[1]=ON TIMEOUT, LBL[10:Timeout]", 10, "Timeout")]
    public void Parse_WaitCondition_WithTimeoutLabelComment_ParsesCorrectly(string input, int expectedNumber, string expectedComment)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        var label = waitCondition.TimeoutLabel;
        var access = Assert.IsType<TpAccessDirect>(label!.LabelNumber);
        Assert.Equal(expectedNumber, access.Number);
        Assert.Equal(expectedComment, access.Comment);
    }

    [Theory]
    [InlineData("WAIT AND R[1]=1")] // AND without left condition
    [InlineData("WAIT OR R[1]=1")] // OR without left condition
    public void Parse_InvalidLogicExpression_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpWaitInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("WAIT  5")]  // Extra space after keyword
    [InlineData("WAIT R[ 1 ] = 1")]  // Spaces inside brackets
    [InlineData("WAIT R[1] = 1  AND  R[2] = 2")]  // Extra spaces around AND
    [InlineData("WAIT DI[1] = ON")]  // Spaces around operator
    [InlineData("WAIT R[1] = 1  TIMEOUT , LBL[1]")]  // Spaces around commas
    public void Parse_WaitInstruction_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("WAIT R[1]=1", 1, TpComparisonOperator.Equal, 1)]
    [InlineData("WAIT R[10]>100", 10, TpComparisonOperator.Greater, 100)]
    public void Parse_WaitCondition_VerifyRegisterAndOperator(string input, int expectedRegister,
        TpComparisonOperator expectedOperator, int expectedValue)
    {
        var result = TpWaitInstruction.GetParser().Parse(input);

        var waitCondition = Assert.IsType<TpWaitCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionSingle>(waitCondition.Condition);
        var comparison = Assert.IsType<TpRegisterComparisonExpression>(logicExpr.Expression);

        var register = Assert.IsType<TpValueRegister>(comparison.Lhs);
        var access = Assert.IsType<TpAccessDirect>(register.Register.Access);
        Assert.Equal(expectedRegister, access.Number);

        Assert.Equal(expectedOperator, comparison.Operator);

        var value = Assert.IsType<TpValueIntegerConstant>(comparison.Rhs);
        Assert.Equal(expectedValue, value.Value);
    }
}
