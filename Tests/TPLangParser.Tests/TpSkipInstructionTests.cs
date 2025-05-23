using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;
using Sprache;

namespace TPLangParser.Tests;

public class TpSkipInstructionTests
{
    [Theory]
    [InlineData("SKIP CONDITION R[1]=1")]
    [InlineData("SKIP CONDITION R[10]=100")]
    [InlineData("SKIP CONDITION R[25]=0")]
    public void Parse_SkipCondition_SimpleRegisterComparison_ParsesCorrectly(string input)
    {
        var result = TpSkipInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var skipCondition = Assert.IsType<TpSkipCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionSingle>(skipCondition.Condition);
        var comparison = Assert.IsType<TpRegisterComparisonExpression>(logicExpr.Expression);
        Assert.Equal(TpComparisonOperator.Equal, comparison.Operator);
    }

    [Theory]
    [InlineData("SKIP CONDITION R[1]<>1")]
    [InlineData("SKIP CONDITION R[10]>100")]
    [InlineData("SKIP CONDITION R[25]<0")]
    [InlineData("SKIP CONDITION R[5]>=10")]
    [InlineData("SKIP CONDITION R[15]<=20")]
    public void Parse_SkipCondition_DifferentOperators_ParsesCorrectly(string input)
    {
        var result = TpSkipInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var skipCondition = Assert.IsType<TpSkipCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionSingle>(skipCondition.Condition);
        var comparison = Assert.IsType<TpRegisterComparisonExpression>(logicExpr.Expression);
        Assert.NotEqual(TpComparisonOperator.Equal, comparison.Operator);
    }

    [Theory]
    [InlineData("SKIP CONDITION R[1]=R[2]")]
    [InlineData("SKIP CONDITION R[10]=AR[5]")]
    public void Parse_SkipCondition_RegisterComparison_ParsesCorrectly(string input)
    {
        var result = TpSkipInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var skipCondition = Assert.IsType<TpSkipCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionSingle>(skipCondition.Condition);
        var comparison = Assert.IsType<TpRegisterComparisonExpression>(logicExpr.Expression);
        Assert.IsType<TpValueRegister>(comparison.Rhs);
    }

    [Theory]
    [InlineData("SKIP CONDITION DI[1]=ON")]
    [InlineData("SKIP CONDITION DO[5]=OFF")]
    [InlineData("SKIP CONDITION RI[3]=ON")]
    public void Parse_SkipCondition_DigitalIOComparison_ParsesCorrectly(string input)
    {
        var result = TpSkipInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var skipCondition = Assert.IsType<TpSkipCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionSingle>(skipCondition.Condition);
        var comparison = Assert.IsType<TpDigitalIOComparisonExpression>(logicExpr.Expression);
        Assert.IsType<TpValueIOState>(comparison.Rhs);
    }

    [Theory]
    [InlineData("SKIP CONDITION AI[1]=5")]
    [InlineData("SKIP CONDITION AO[5]>10")]
    [InlineData("SKIP CONDITION GI[3]<100")]
    public void Parse_SkipCondition_AnalogIOComparison_ParsesCorrectly(string input)
    {
        var result = TpSkipInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var skipCondition = Assert.IsType<TpSkipCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionSingle>(skipCondition.Condition);
        var comparison = Assert.IsType<TpAnalogIOComparisonExpression>(logicExpr.Expression);
        Assert.IsType<TpValueIntegerConstant>(comparison.Rhs);
    }

    [Theory]
    [InlineData("SKIP CONDITION $GROUP[1].$CURRENT_ANG[2]=0")]
    [InlineData("SKIP CONDITION $TIMER[1]>10")]
    public void Parse_SkipCondition_ParameterComparison_ParsesCorrectly(string input)
    {
        var result = TpSkipInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var skipCondition = Assert.IsType<TpSkipCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionSingle>(skipCondition.Condition);
        var comparison = Assert.IsType<TpParameterComparisonExpression>(logicExpr.Expression);
    }

    [Theory]
    [InlineData("SKIP CONDITION R[1]=1 AND R[2]=2")]
    [InlineData("SKIP CONDITION DI[1]=ON AND DO[2]=OFF")]
    [InlineData("SKIP CONDITION R[1]>10 AND AI[1]<50")]
    public void Parse_SkipCondition_AndCondition_ParsesCorrectly(string input)
    {
        var result = TpSkipInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var skipCondition = Assert.IsType<TpSkipCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionAnd>(skipCondition.Condition);
        Assert.Equal(2, logicExpr.Expression.Count);
    }

    [Theory]
    [InlineData("SKIP CONDITION R[1]=1 OR R[2]=2")]
    [InlineData("SKIP CONDITION DI[1]=ON OR DO[2]=OFF")]
    [InlineData("SKIP CONDITION R[1]>10 OR AI[1]<50")]
    public void Parse_SkipCondition_OrCondition_ParsesCorrectly(string input)
    {
        var result = TpSkipInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var skipCondition = Assert.IsType<TpSkipCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionOr>(skipCondition.Condition);
        Assert.Equal(2, logicExpr.Expression.Count);
    }

    [Theory]
    [InlineData("SKIP CONDITION R[1]=1 AND R[2]=2 AND R[3]=3")]
    [InlineData("SKIP CONDITION DI[1]=ON AND DO[2]=OFF AND RI[3]=ON")]
    public void Parse_SkipCondition_MultipleAndConditions_ParsesCorrectly(string input)
    {
        var result = TpSkipInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var skipCondition = Assert.IsType<TpSkipCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionAnd>(skipCondition.Condition);
        Assert.Equal(3, logicExpr.Expression.Count);
    }

    [Theory]
    [InlineData("SKIP CONDITION R[1]=1 OR R[2]=2 OR R[3]=3")]
    [InlineData("SKIP CONDITION DI[1]=ON OR DO[2]=OFF OR RI[3]=ON")]
    public void Parse_SkipCondition_MultipleOrConditions_ParsesCorrectly(string input)
    {
        var result = TpSkipInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var skipCondition = Assert.IsType<TpSkipCondition>(result);
        var logicExpr = Assert.IsType<TpLogicExpressionOr>(skipCondition.Condition);
        Assert.Equal(3, logicExpr.Expression.Count);
    }

    [Theory]
    [InlineData("SKIP CONDITION")] // Missing condition
    [InlineData("SKIP")] // Incomplete keyword
    [InlineData("SKIP CONDITION R[1]")] // Missing operator and right-hand value
    [InlineData("SKIP CONDITION R[1]=")] // Missing right-hand value
    [InlineData("SKIP CONDITION =1")] // Missing left-hand value
    public void Parse_InvalidSkipInstruction_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpSkipInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("SKIP CONDITION AND R[1]=1")] // AND without left condition
    [InlineData("SKIP CONDITION OR R[1]=1")] // OR without left condition
    public void Parse_InvalidLogicExpression_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpSkipInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("SKIP CONDITION  R[1] = 1")]  // Extra space after keyword
    [InlineData("SKIP CONDITION R[ 1 ] = 1")]  // Spaces inside brackets
    [InlineData("SKIP CONDITION R[1] = 1  AND  R[2] = 2")]  // Extra spaces around AND
    [InlineData("SKIP CONDITION DI[1] = ON")]  // Spaces around operator
    public void Parse_SkipInstruction_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpSkipInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("skip condition R[1]=1")]  // Lowercase keywords
    [InlineData("SKIP condition R[1]=1")]  // Mixed case keywords
    [InlineData("SKIP CONDITION r[1]=1")]  // Lowercase register
    public void Parse_SkipInstruction_CaseSensitive_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpSkipInstruction.GetParser().Parse(input));
}
