using AGT.TPLangParser.TPLang;
using AGT.TPLangParser.TPLang.Instructions;
using Sprache;

namespace AGT.TPLangParser.Tests;

public class TpMixedLogicInstructionTests
{
    [Theory]
    [InlineData("R[1]=(DI[1] AND DI[2])")]
    [InlineData("R[10]=(DO[1] OR DO[2])")]
    [InlineData("R[5]=(R[1]>0 AND R[2]<10)")]
    [InlineData("R[1]=(DI[1] AND (DI[2] OR DI[3]))")]
    [InlineData("R[34:MathTemp]=($[AGT].CONF.EXT_AXIS.RAIL.POS-(R[308:ER_RailMin]-$[AGT]SAFE_DIST.TOTAL_SOFT))")]
    public void Parse_MixedLogicAssignment_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicAssignment>(result);

        var assignment = (TpMixedLogicAssignment)result;
        Assert.NotNull(assignment.Assignable);
        Assert.NotNull(assignment.Expression);
    }

    [Theory]
    [InlineData("WAIT (DI[1]=ON)")]
    [InlineData("WAIT (DI[1] AND DI[2])")]
    [InlineData("WAIT (R[1]>0)")]
    [InlineData("WAIT (DI[1] AND (DI[2] OR DI[3]))")]
    public void Parse_MixedLogicWaitInstruction_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicWaitInstruction>(result);

        var wait = (TpMixedLogicWaitInstruction)result;
        Assert.NotNull(wait.Expression);
    }

    [Theory]
    [InlineData("R[1]=(DI[1] AND DI[2])", typeof(TpRegister), 1)]
    [InlineData("PR[5]=(R[1]>0)", typeof(TpPositionRegister), 5)]
    [InlineData("SR[1]=(DI[1] OR DI[2])", typeof(TpStringRegister), 1)]
    public void Parse_MixedLogicAssignment_VerifyAssignable(string input, Type expectedType, int expectedNumber)
    {
        var result = TpMixedLogicInstruction.GetParser().Parse(input);
        var assignment = (TpMixedLogicAssignment)result;

        // Verify the type is correct
        var registerValue = Assert.IsType<TpValueRegister>(assignment.Assignable);

        // Verify register details
        Assert.IsType(expectedType, registerValue.Register);
        var access = Assert.IsType<TpAccessDirect>(registerValue.Register.Access);

        // Verify register number
        Assert.Equal(expectedNumber, access.Number);
    }

    [Theory]
    [InlineData("R[1]=(R[2]+5>10)")]
    [InlineData("R[1]=(R[2]*2=R[3]/2)")]
    [InlineData("R[1]=(R[2]+R[3]>R[4]-R[5])")]
    public void Parse_MixedLogicAssignment_WithArithmeticExpressions_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicAssignment>(result);
    }

    [Theory]
    [InlineData("WAIT (DI[1]=ON AND R[1]>5)")]
    [InlineData("WAIT (DO[1]=OFF OR R[2]<10)")]
    [InlineData("WAIT (R[1]+R[2]>R[3]*2)")]
    public void Parse_MixedLogicWait_WithComplexConditions_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
        Assert.IsType<TpMixedLogicWaitInstruction>(result);
    }

    [Theory]
    [InlineData("R[1]=()")] // Empty expression
    [InlineData("R[1]=(DI[1] AND)")] // Incomplete AND
    [InlineData("R[1]=(DI[1] OR)")] // Incomplete OR
    [InlineData("WAIT DI[1]")] // Missing parentheses
    [InlineData("WAIT ()")] // Empty wait condition
    [InlineData("WAIT (AND DI[1])")] // Invalid operator position
    public void Parse_InvalidMixedLogicInstruction_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpMixedLogicInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("R[1] = (DI[1] AND DI[2])")] // Spaces around equals
    [InlineData("R[1]=(DI[1]  AND  DI[2])")] // Extra spaces around AND
    [InlineData("WAIT  (DI[1])")] // Extra space after WAIT
    [InlineData("WAIT(  DI[1]  )")] // Spaces inside parentheses
    public void Parse_MixedLogicInstruction_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpMixedLogicInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("r[1]=(DI[1] AND DI[2])")] // Lowercase register
    [InlineData("R[1]=(di[1] AND DI[2])")] // Lowercase DI
    [InlineData("R[1]=(DI[1] and DI[2])")] // Lowercase operator
    [InlineData("wait (DI[1])")] // Lowercase WAIT
    public void Parse_MixedLogicInstruction_CaseInsensitive_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpMixedLogicInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("R[1]=(DI[1] AND (DI[2] OR DI[3]))")] // Nested AND/OR
    [InlineData("R[1]=((DI[1] OR DI[2]) AND DI[3])")] // Nested with outer operation
    [InlineData("WAIT ((R[1]>0 AND R[2]<10) OR DI[1])")] // Complex nested condition
    public void Parse_MixedLogicInstruction_WithNestedExpressions_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("R[1]=(DI[1] AND DI[2] AND DI[3])")] // Multiple AND
    [InlineData("R[1]=(DI[1] OR DI[2] OR DI[3])")] // Multiple OR
    [InlineData("WAIT (DI[1] AND DI[2] AND DI[3] AND DI[4])")] // Four conditions
    public void Parse_MixedLogicInstruction_WithMultipleOperators_ParsesCorrectly(string input)
    {
        var result = TpMixedLogicInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }
}
