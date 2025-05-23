using Sprache;
using TPLangParser.TPLang;

namespace TPLangParser.Tests;

public class TpMixedLogicTests
{
    [Theory]
    [InlineData("DI[1]")]
    [InlineData("(DI[1])")]
    public void Parse_DigitalInput_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIOPort>(value.Value);
        var ioPortValue = (TpValueIOPort)value.Value;
        Assert.IsType<TpDigitalIOPort>(ioPortValue.IOPort);
        var port = (TpDigitalIOPort)ioPortValue.IOPort;
        Assert.Equal(TpIOType.Input, port.Type);
        Assert.IsType<TpAccessDirect>(port.PortNumber);
        var directAccess = (TpAccessDirect)port.PortNumber;
        Assert.Equal(1, directAccess.Number);
    }

    [Theory]
    [InlineData("DO[5]")]
    [InlineData("(DO[5])")]
    public void Parse_DigitalOutput_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIOPort>(value.Value);
        var ioPortValue = (TpValueIOPort)value.Value;
        Assert.IsType<TpDigitalIOPort>(ioPortValue.IOPort);
        var port = (TpDigitalIOPort)ioPortValue.IOPort;
        Assert.Equal(TpIOType.Output, port.Type);
        Assert.IsType<TpAccessDirect>(port.PortNumber);
        var directAccess = (TpAccessDirect)port.PortNumber;
        Assert.Equal(5, directAccess.Number);
    }

    [Theory]
    [InlineData("RI[1]")]
    [InlineData("(RI[1])")]
    public void Parse_RobotInput_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIOPort>(value.Value);
        var ioPortValue = (TpValueIOPort)value.Value;
        Assert.IsType<TpRobotIOPort>(ioPortValue.IOPort);
        var port = (TpRobotIOPort)ioPortValue.IOPort;
        Assert.Equal(TpIOType.Input, port.Type);
        Assert.IsType<TpAccessDirect>(port.PortNumber);
        var directAccess = (TpAccessDirect)port.PortNumber;
        Assert.Equal(1, directAccess.Number);
    }

    [Theory]
    [InlineData("RO[5]")]
    [InlineData("(RO[5])")]
    public void Parse_RobotOutput_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIOPort>(value.Value);
        var ioPortValue = (TpValueIOPort)value.Value;
        Assert.IsType<TpRobotIOPort>(ioPortValue.IOPort);
        var port = (TpRobotIOPort)ioPortValue.IOPort;
        Assert.Equal(TpIOType.Output, port.Type);
        Assert.IsType<TpAccessDirect>(port.PortNumber);
        var directAccess = (TpAccessDirect)port.PortNumber;
        Assert.Equal(5, directAccess.Number);
    }

    [Theory]
    [InlineData("UI[1]")]
    [InlineData("(UI[1])")]
    public void Parse_UopInput_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIOPort>(value.Value);
        var ioPortValue = (TpValueIOPort)value.Value;
        Assert.IsType<TpUopIOPort>(ioPortValue.IOPort);
        var port = (TpUopIOPort)ioPortValue.IOPort;
        Assert.Equal(TpIOType.Input, port.Type);
        Assert.IsType<TpAccessDirect>(port.PortNumber);
        var directAccess = (TpAccessDirect)port.PortNumber;
        Assert.Equal(1, directAccess.Number);
    }

    [Theory]
    [InlineData("UO[5]")]
    [InlineData("(UO[5])")]
    public void Parse_UopOutput_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIOPort>(value.Value);
        var ioPortValue = (TpValueIOPort)value.Value;
        Assert.IsType<TpUopIOPort>(ioPortValue.IOPort);
        var port = (TpUopIOPort)ioPortValue.IOPort;
        Assert.Equal(TpIOType.Output, port.Type);
        Assert.IsType<TpAccessDirect>(port.PortNumber);
        var directAccess = (TpAccessDirect)port.PortNumber;
        Assert.Equal(5, directAccess.Number);
    }

    [Theory]
    [InlineData("SI[1]")]
    [InlineData("(SI[1])")]
    public void Parse_SoInput_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIOPort>(value.Value);
        var ioPortValue = (TpValueIOPort)value.Value;
        Assert.IsType<TpSopIOPort>(ioPortValue.IOPort);
        var port = (TpSopIOPort)ioPortValue.IOPort;
        Assert.Equal(TpIOType.Input, port.Type);
        Assert.IsType<TpAccessDirect>(port.PortNumber);
        var directAccess = (TpAccessDirect)port.PortNumber;
        Assert.Equal(1, directAccess.Number);
    }

    [Theory]
    [InlineData("(SO[5])")]
    public void Parse_SoOutput_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIOPort>(value.Value);
        var ioPortValue = (TpValueIOPort)value.Value;
        Assert.IsType<TpSopIOPort>(ioPortValue.IOPort);
        var port = (TpSopIOPort)ioPortValue.IOPort;
        Assert.Equal(TpIOType.Output, port.Type);
        Assert.IsType<TpAccessDirect>(port.PortNumber);
        var directAccess = (TpAccessDirect)port.PortNumber;
        Assert.Equal(5, directAccess.Number);
    }

    [Theory]
    [InlineData("GI[1]")]
    [InlineData("(GI[1])")]
    public void Parse_GroupInput_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIOPort>(value.Value);
        var ioPortValue = (TpValueIOPort)value.Value;
        Assert.IsType<TpGroupIOPort>(ioPortValue.IOPort);
        var port = (TpGroupIOPort)ioPortValue.IOPort;
        Assert.Equal(TpIOType.Input, port.Type);
        Assert.IsType<TpAccessDirect>(port.PortNumber);
        var directAccess = (TpAccessDirect)port.PortNumber;
        Assert.Equal(1, directAccess.Number);
    }

    [Theory]
    [InlineData("GO[1]")]
    [InlineData("(GO[1])")]
    public void Parse_GroupOutput_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIOPort>(value.Value);
        var ioPortValue = (TpValueIOPort)value.Value;
        Assert.IsType<TpGroupIOPort>(ioPortValue.IOPort);
        var port = (TpGroupIOPort)ioPortValue.IOPort;
        Assert.Equal(TpIOType.Output, port.Type);
        Assert.IsType<TpAccessDirect>(port.PortNumber);
        var directAccess = (TpAccessDirect)port.PortNumber;
        Assert.Equal(1, directAccess.Number);
    }

    [Theory]
    [InlineData("AI[1]")]
    [InlineData("(AI[1])")]
    public void Parse_AnalogInput_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIOPort>(value.Value);
        var ioPortValue = (TpValueIOPort)value.Value;
        Assert.IsType<TpAnalogIOPort>(ioPortValue.IOPort);
        var port = (TpAnalogIOPort)ioPortValue.IOPort;
        Assert.Equal(TpIOType.Input, port.Type);
        Assert.IsType<TpAccessDirect>(port.PortNumber);
        var directAccess = (TpAccessDirect)port.PortNumber;
        Assert.Equal(1, directAccess.Number);
    }

    [Theory]
    [InlineData("AO[1]")]
    [InlineData("(AO[1])")]
    public void Parse_AnalogOutput_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIOPort>(value.Value);
        var ioPortValue = (TpValueIOPort)value.Value;
        Assert.IsType<TpAnalogIOPort>(ioPortValue.IOPort);
        var port = (TpAnalogIOPort)ioPortValue.IOPort;
        Assert.Equal(TpIOType.Output, port.Type);
        Assert.IsType<TpAccessDirect>(port.PortNumber);
        var directAccess = (TpAccessDirect)port.PortNumber;
        Assert.Equal(1, directAccess.Number);
    }

    [Theory]
    [InlineData("F[1]")]
    [InlineData("(F[1])")]
    public void Parse_Flag_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueFlag>(value.Value);
        var flag = (TpValueFlag)value.Value;
        Assert.IsType<TpAccessDirect>(flag.Flag.Access);
        var directAccess = (TpAccessDirect)flag.Flag.Access;
        Assert.Equal(1, directAccess.Number);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("42", 42)]
    [InlineData("100", 100)]
    [InlineData("(-1)", -1)]
    [InlineData("(-100)", -100)]
    [InlineData("2147483647", 2147483647)]  // Int32.MaxValue
    [InlineData("(-2147483647)", -2147483647)]  // Int32.MinValue
    public void Parse_IntegerConstant_ParsesCorrectly(string input, int expectedValue)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIntegerConstant>(value.Value);

        var intConstant = (TpValueIntegerConstant)value.Value;
        Assert.Equal(expectedValue, intConstant.Value);
    }

    [Theory]
    [InlineData("0.0", 0.0)]
    [InlineData("1.0", 1.0)]
    [InlineData("42.0", 42.0)]
    [InlineData("3.14159", 3.14159)]
    [InlineData("(-1.5)", -1.5)]
    [InlineData("(-100.25)", -100.25)]
    [InlineData("0.001", 0.001)]
    [InlineData("1000.001", 1000.001)]
    public void Parse_FloatingPointConstant_ParsesCorrectly(string input, double expectedValue)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueFloatingPointConstant>(value.Value);

        var floatConstant = (TpValueFloatingPointConstant)value.Value;
        Assert.Equal(expectedValue, floatConstant.Value);
    }

    [Theory]
    [InlineData("$ERROR", typeof(TpValueSystemVariable))]
    [InlineData("$ERROR[1]", typeof(TpValueSystemVariable))]
    [InlineData("$RESUME.$TEST[2].$VAR", typeof(TpValueSystemVariable))]
    [InlineData("$[AGT]CONF", typeof(TpValueKarelVariable))]
    [InlineData("$[AGT]CONF.EXT_AXIS.RAIL.ENB", typeof(TpValueKarelVariable))]
    [InlineData("$[AGT]CONF.ROBOT_MODEL", typeof(TpValueKarelVariable))]
    public void Parse_SystemVariable_ParsesCorrectly(string input, Type expectedType)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);
        var tpMixedLogicValue = (TpMixedLogicValue)result;
        Assert.IsType(expectedType, tpMixedLogicValue.Value);
    }

    [Theory]
    [InlineData("R[1]>0", TpComparisonOperator.Greater)]
    [InlineData("R[5]<$[PROG]VAL", TpComparisonOperator.Lesser)]
    [InlineData("R[10]=GI[3]", TpComparisonOperator.Equal)]
    [InlineData("100>=R[15]", TpComparisonOperator.GreaterEqual)]
    [InlineData("R[20]<=AI[1]", TpComparisonOperator.LesserEqual)]
    [InlineData("AO[2]<>0", TpComparisonOperator.NotEqual)]
    [InlineData("AO[2]<>(5 + $SOMEVAR)", TpComparisonOperator.NotEqual)]
    public void Parse_ComparisonExpression_ParsesCorrectly(string input, TpComparisonOperator expectedOperator)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicBinaryComparison>(result);
        var binaryExpr = (TpMixedLogicBinaryComparison)result;
        Assert.Equal(expectedOperator, binaryExpr.Operator);
    }

    [Theory]
    [InlineData("DI[1] AND DO[2]",  TpLogicalOperator.And)]
    [InlineData("RI[3] OR RO[4]",  TpLogicalOperator.Or)]
    [InlineData("F[1] AND !F[2]", TpLogicalOperator.And)]
    public void Parse_LogicalExpression_ParsesCorrectly(
        string input,
        TpLogicalOperator expectedOperator)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicBinaryLogical>(result);
        var logicalExpr = (TpMixedLogicBinaryLogical)result;
        Assert.Equal(expectedOperator, logicalExpr.Operator);
    }

    [Theory]
    [InlineData("1+1", TpArithmeticOperator.Plus)]
    [InlineData("1-1", TpArithmeticOperator.Minus)]
    [InlineData("1*1", TpArithmeticOperator.Times)]
    [InlineData("1/1", TpArithmeticOperator.Div)]
    [InlineData("1MOD1", TpArithmeticOperator.Mod)]
    [InlineData("1DIV1", TpArithmeticOperator.IntegerDiv)]
    [InlineData("1.1+(-1)", TpArithmeticOperator.Plus)]
    [InlineData("1-$VAR.VAL", TpArithmeticOperator.Minus)]
    [InlineData("1*GI[1]", TpArithmeticOperator.Times)]
    [InlineData("AO[5]/(-1)", TpArithmeticOperator.Div)]
    public void Parse_ArithmeticalExpression_ParsesCorrectly(
        string input,
        TpArithmeticOperator expectedOperator)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicBinaryArithmetic>(result);
        var arithExpr = (TpMixedLogicBinaryArithmetic)result;
        Assert.Equal(expectedOperator, arithExpr.Operator);
    }

    [Theory]
    [InlineData("!DI[1]")]
    [InlineData("!DO[5]")]
    [InlineData("!RI[3]")]
    [InlineData("!RO[4]")]
    [InlineData("!UI[2]")]
    [InlineData("!UO[6]")]
    [InlineData("!SI[7]")]
    [InlineData("!SO[8]")]
    [InlineData("!F[9]")]
    [InlineData("!GI[1]")]
    [InlineData("!GO[3]")]
    [InlineData("!$ERROR[1]")]
    [InlineData("!(DI[1] AND DO[2])")]
    [InlineData("!(R[1]>0)")]
    public void Parse_UnaryNegation_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicUnaryNot>(result);

        var negation = (TpMixedLogicUnaryNot)result;
        Assert.NotNull(negation.Term);
    }

    [Theory]
    [InlineData("!!DI[1]")]  // Double negation, TODO: ensure this is actually supported
    [InlineData("!(!(DI[1] AND DO[2]))")]  // Nested double negation
    [InlineData("!(!F[9])")]  // Nested negation
    public void Parse_NestedNegation_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicUnaryNot>(result);

        var outerNegation = (TpMixedLogicUnaryNot)result;
        Assert.NotNull(outerNegation.Term);
        Assert.IsType<TpMixedLogicUnaryNot>(outerNegation.Term);
    }

    [Theory]
    [InlineData("!(DI[1] AND DO[2]) OR RI[3]")]
    [InlineData("!DI[1] AND DO[2]")]
    [InlineData("DI[1] AND !DO[2]")]
    [InlineData("!R[1]>0")]  // Negation of a register comparison
    [InlineData("!(R[1]>0)")]  // Negation of a parenthesized register comparison
    public void Parse_NegationWithLogicalOperators_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);

        Assert.NotNull(result);
        // We're just checking that it parses without exceptions
        // The exact structure depends on the operator precedence rules
    }

    [Theory]
    [InlineData("(((DI[1] AND DO[2]) OR SI[3]) AND SO[4])")]
    [InlineData("(DI[1] AND (DO[2] OR (SI[3] AND SO[4])))")]
    [InlineData("((R[1]>0) AND ((R[2]<5) OR (R[3]=0)))")]
    [InlineData("(((1+2)*3)>((4-5)/6))")]
    [InlineData("((R[1]+(R[2]*R[3]))>=(R[4]-(R[5]/R[6])))")]
    public void Parse_DeeplyNestedExpression_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);
        
        Assert.NotNull(result);
        // We're primarily checking that deeply nested expressions parse correctly
        // The exact structure validation would be quite complex and is covered by other tests
    }
    [Theory]
    [InlineData("ON")]
    [InlineData("(ON)")]
    [InlineData("OFF")]
    [InlineData("(OFF)")]
    public void Parse_IOState_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicValue>(result);

        var value = (TpMixedLogicValue)result;
        Assert.IsType<TpValueIOState>(value.Value);

        var ioState = (TpValueIOState)value.Value;
        var expectedState = input.Contains("ON") ? TpOnOffState.On : TpOnOffState.Off;
        Assert.Equal(expectedState, ioState.State);
    }

    [Theory]
    [InlineData("DI[1]=ON", TpComparisonOperator.Equal, TpOnOffState.On)]
    [InlineData("DO[5]=OFF", TpComparisonOperator.Equal, TpOnOffState.Off)]
    [InlineData("RI[3]<>ON", TpComparisonOperator.NotEqual, TpOnOffState.On)]
    [InlineData("RO[4]<>OFF", TpComparisonOperator.NotEqual, TpOnOffState.Off)]
    [InlineData("UI[2]=ON", TpComparisonOperator.Equal, TpOnOffState.On)]
    [InlineData("UO[6]=OFF", TpComparisonOperator.Equal, TpOnOffState.Off)]
    [InlineData("SI[7]=ON", TpComparisonOperator.Equal, TpOnOffState.On)]
    [InlineData("SO[8]=OFF", TpComparisonOperator.Equal, TpOnOffState.Off)]
    [InlineData("GI[1]=ON", TpComparisonOperator.Equal, TpOnOffState.On)]
    [InlineData("GO[3]=OFF", TpComparisonOperator.Equal, TpOnOffState.Off)]
    public void Parse_IOPortComparedToIOState_ParsesCorrectly(
        string input,
        TpComparisonOperator expectedOperator,
        TpOnOffState expectedState)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicBinaryComparison>(result);

        var comparison = (TpMixedLogicBinaryComparison)result;
        Assert.Equal(expectedOperator, comparison.Operator);

        // Left side should be an IO port
        Assert.IsType<TpMixedLogicValue>(comparison.Lhs);
        var leftValue = (TpMixedLogicValue)comparison.Lhs;
        Assert.IsType<TpValueIOPort>(leftValue.Value);

        // Right side should be an IO state
        Assert.IsType<TpMixedLogicValue>(comparison.Rhs);
        var rightValue = (TpMixedLogicValue)comparison.Rhs;
        Assert.IsType<TpValueIOState>(rightValue.Value);

        var ioState = (TpValueIOState)rightValue.Value;
        Assert.Equal(expectedState, ioState.State);
    }

    [Theory]
    [InlineData("ON=DI[1]", TpComparisonOperator.Equal, TpOnOffState.On)]
    [InlineData("OFF=DO[5]", TpComparisonOperator.Equal, TpOnOffState.Off)]
    [InlineData("ON<>RI[3]", TpComparisonOperator.NotEqual, TpOnOffState.On)]
    [InlineData("OFF<>RO[4]", TpComparisonOperator.NotEqual, TpOnOffState.Off)]
    public void Parse_IOStateComparedToIOPort_ParsesCorrectly(
        string input,
        TpComparisonOperator expectedOperator,
        TpOnOffState expectedState)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicBinaryComparison>(result);

        var comparison = (TpMixedLogicBinaryComparison)result;
        Assert.Equal(expectedOperator, comparison.Operator);

        // Left side should be an IO state
        Assert.IsType<TpMixedLogicValue>(comparison.Lhs);
        var leftValue = (TpMixedLogicValue)comparison.Lhs;
        Assert.IsType<TpValueIOState>(leftValue.Value);

        var ioState = (TpValueIOState)leftValue.Value;
        Assert.Equal(expectedState, ioState.State);

        // Right side should be an IO port
        Assert.IsType<TpMixedLogicValue>(comparison.Rhs);
        var rightValue = (TpMixedLogicValue)comparison.Rhs;
        Assert.IsType<TpValueIOPort>(rightValue.Value);
    }


    [Theory]
    [InlineData("(DI[1]=ON) AND (DO[2]=OFF)")]
    public void Parse_ComplexExpressionsWithIOStates_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicExpression.GetParser().Parse(input);

        Assert.NotNull(result);
        // Primarily checking that complex expressions with IO states parse correctly
        // Detailed structure validation covered in other tests
    }
}
