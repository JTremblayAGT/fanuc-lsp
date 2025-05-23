using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;
using Sprache;

namespace TPLangParser.Tests;

public class TpPosRegInstructionTests
{
    [Theory]
    [InlineData("PR[1]=P[1]")]
    [InlineData("PR[5]=LPOS")]
    [InlineData("PR[10]=JPOS")]
    [InlineData("PR[62:JPOS]=JPOS")]
    [InlineData("PR[63:Cartesian_Temp]=P[3]")]
    public void Parse_PosRegAssignment_Simple_ParsesCorrectly(string input)
    {
        var result = TpPosRegInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var assignment = Assert.IsType<TpPosRegAssignmentInstruction>(result);
        Assert.NotNull(assignment.PosReg);
        Assert.IsType<TpPosRegValue>(assignment.Expr);
    }

    [Theory]
    [InlineData("PR[1]=P[1]", 1)]
    [InlineData("PR[5]=LPOS", 5)]
    [InlineData("PR[10]=PR[2]", 10)]
    public void Parse_PosRegAssignment_VerifyRegisterNumber(string input, int expectedNumber)
    {
        var result = TpPosRegInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpPosRegAssignmentInstruction>(result);
        var access = Assert.IsType<TpAccessDirect>(assignment.PosReg.Access);
        Assert.Equal(expectedNumber, access.Number);
    }

    [Theory]
    [InlineData("PR[1]=P[1]+P[2]")]
    [InlineData("PR[5]=LPOS+PR[1]")]
    [InlineData("PR[10]=PR[2]+UFRAME[1]")]
    public void Parse_PosRegAssignment_Addition_ParsesCorrectly(string input)
    {
        var result = TpPosRegInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpPosRegAssignmentInstruction>(result);
        var addition = Assert.IsType<TpPosRegAddition>(assignment.Expr);
        Assert.NotNull(addition.Lhs);
        Assert.NotNull(addition.Rhs);
    }

    [Theory]
    [InlineData("PR[1]=P[1]-P[2]")]
    [InlineData("PR[5]=LPOS-PR[1]")]
    [InlineData("PR[10]=PR[2]-UFRAME[1]")]
    public void Parse_PosRegAssignment_Subtraction_ParsesCorrectly(string input)
    {
        var result = TpPosRegInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpPosRegAssignmentInstruction>(result);
        var subtraction = Assert.IsType<TpPosRegSubtraction>(assignment.Expr);
        Assert.NotNull(subtraction.Lhs);
        Assert.NotNull(subtraction.Rhs);
    }

    [Theory]
    [InlineData("PR[1]=P[1]+P[2]-P[3]")]
    [InlineData("PR[5]=LPOS+PR[1]-JPOS")]
    public void Parse_PosRegAssignment_ComplexExpression_ParsesCorrectly(string input)
    {
        var result = TpPosRegInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpPosRegAssignmentInstruction>(result);
        Assert.NotNull(assignment.Expr);
        // The second operation becomes the Rhs of the first operation
        Assert.IsType<TpPosRegAddition>(assignment.Expr);
        Assert.IsType<TpPosRegSubtraction>(((TpPosRegAddition)assignment.Expr).Rhs);
    }

    [Theory]
    [InlineData("PR[1,1]=5")]
    [InlineData("PR[5,2]=10")]
    [InlineData("PR[10,6]=100")]
    [InlineData("PR[50,4:CAM3D RESULT]=0")]
    public void Parse_PosRegElementAssignment_Simple_ParsesCorrectly(string input)
    {
        var result = TpPosRegInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var assignment = Assert.IsType<TpPosRegElementAssignmentInstruction>(result);
        Assert.NotNull(assignment.PosReg);
        Assert.IsType<TpArithmeticValue>(assignment.Expr);
    }

    [Theory]
    [InlineData("PR[1,1]=R[1]")]
    [InlineData("PR[5,2]=DI[1]")]
    [InlineData("PR[10,6]=5.5")]
    public void Parse_PosRegElementAssignment_VerifyValue(string input)
    {
        var result = TpPosRegInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpPosRegElementAssignmentInstruction>(result);
        var value = Assert.IsType<TpArithmeticValue>(assignment.Expr);
        Assert.NotNull(value.Value);
    }

    [Theory]
    [InlineData("PR[1,1]=R[1]+5")]
    [InlineData("PR[5,2]=10+R[1]")]
    [InlineData("PR[10,6]=R[1]+R[2]")]
    public void Parse_PosRegElementAssignment_Addition_ParsesCorrectly(string input)
    {
        var result = TpPosRegInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpPosRegElementAssignmentInstruction>(result);
        Assert.IsType<TpArithmeticAddition>(assignment.Expr);
    }

    [Theory]
    [InlineData("PR[1,1]=R[1]*5")]
    [InlineData("PR[5,2]=10/R[1]")]
    [InlineData("PR[10,6]=R[1]MOD5")]
    public void Parse_PosRegElementAssignment_OtherOperations_ParsesCorrectly(string input)
    {
        var result = TpPosRegInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpPosRegElementAssignmentInstruction>(result);
        Assert.IsAssignableFrom<TpArithmeticBinary>(assignment.Expr);
    }

    [Theory]
    [InlineData("PR[1,1]=R[1]+5*2")]
    [InlineData("PR[5,2]=10+R[1]/5")]
    [InlineData("PR[10,6]=R[1]*R[2]+R[3]")]
    public void Parse_PosRegElementAssignment_ComplexExpression_ParsesCorrectly(string input)
    {
        var result = TpPosRegInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpPosRegElementAssignmentInstruction>(result);
        Assert.NotNull(assignment.Expr);
        Assert.IsAssignableFrom<TpArithmeticBinary>(assignment.Expr);
    }

    [Theory]
    [InlineData("LOCK PREG")]
    public void Parse_LockPreg_ParsesCorrectly(string input)
    {
        var result = TpPosRegInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpLockPregInstruction>(result);
    }

    [Theory]
    [InlineData("UNLOCK PREG")]
    public void Parse_UnlockPreg_ParsesCorrectly(string input)
    {
        var result = TpPosRegInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpUnlockPregInstruction>(result);
    }

    [Theory]
    [InlineData("PR[1]=")] // Missing expression
    [InlineData("PR=P[1]")] // Invalid PR format
    [InlineData("PR[1,]=")] // Invalid element format
    [InlineData("PR[,1]=")] // Invalid element format
    [InlineData("LOCK")] // Incomplete lock
    [InlineData("UNLOCK")] // Incomplete unlock
    public void Parse_InvalidPosRegInstruction_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpPosRegInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("PR[1] = P[1]")]  // Spaces around equals
    [InlineData("PR[ 1 ] = P[ 2 ]")]  // Spaces inside brackets
    [InlineData("PR[1] = P[1] + P[2]")]  // Spaces around operator
    [InlineData("PR[1,1] = 5")]  // Spaces in element assignment
    public void Parse_PosRegInstruction_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpPosRegInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }
}