using AGT.TPLangParser.TPLang;
using AGT.TPLangParser.TPLang.Instructions;
using Sprache;

namespace AGT.TPLangParser.Tests;

public class TpBranchingInstructionTests
{
    [Theory]
    [InlineData("LBL[1]", 1)]
    [InlineData("LBL[100]", 100)]
    [InlineData("LBL[999]", 999)]
    [InlineData("LBL[1011:Zone A UF[4]]", 1011)]
    [InlineData("LBL[1011:UF[4] Zone A]", 1011)]
    public void Parse_LabelDefinition_ParsesCorrectly(string input, int expectedLabelNumber)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpLabelDefinitionInstruction>(result);

        var labelDef = (TpLabelDefinitionInstruction)result;
        Assert.NotNull(labelDef.Label);
        var access = Assert.IsType<TpAccessDirect>(labelDef.Label.LabelNumber);
        Assert.Equal(expectedLabelNumber, access.Number);
    }

    [Theory]
    [InlineData("END")]
    public void Parse_EndInstruction_ParsesCorrectly(string input)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpEndInstruction>(result);
    }

    [Theory]
    [InlineData("JMP LBL[1]", 1)]
    [InlineData("JMP LBL[50]", 50)]
    [InlineData("JMP LBL[999]", 999)]
    public void Parse_JumpLabelInstruction_ParsesCorrectly(string input, int expectedLabelNumber)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpJumpLabelInstruction>(result);

        var jump = (TpJumpLabelInstruction)result;
        Assert.NotNull(jump.Label);
        var direct = Assert.IsType<TpAccessDirect>(jump.Label.LabelNumber);
        Assert.Equal(expectedLabelNumber, direct.Number);
    }

    [Theory]
    [InlineData("JMP LBL[R[1]]", 1)]
    [InlineData("JMP LBL[R[50]]", 50)]
    [InlineData("JMP LBL[R[999]]", 999)]
    [InlineData("JMP LBL[R[213]]", 213)]
    public void Parse_JumpLabelInstructionIndirect_ParsesCorrectly(string input, int expectedLabelNumber)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpJumpLabelInstruction>(result);

        var jump = (TpJumpLabelInstruction)result;
        Assert.NotNull(jump.Label);
        var indirect = Assert.IsType<TpAccessIndirect>(jump.Label.LabelNumber);
        var direct = Assert.IsType<TpAccessDirect>(indirect.Register.Access);
        Assert.Equal(expectedLabelNumber, direct.Number);
    }

    [Theory]
    [InlineData("CALL TEST", "TEST")]
    [InlineData("CALL MAIN", "MAIN")]
    [InlineData("CALL PROGRAM_1", "PROGRAM_1")]
    [InlineData("CALL MY_ROUTINE", "MY_ROUTINE")]
    public void Parse_CallInstruction_ParsesCorrectly(string input, string expectedProgramName)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var call = Assert.IsType<TpCallInstruction>(result);
        var method = Assert.IsType<TpCallByName>(call.CallMethod);
        Assert.Equal(expectedProgramName, method.ProgramName);
    }

    [Theory]
    [InlineData("IF R[1]>0, JMP LBL[1]")]
    [InlineData("IF R[10]=5, CALL TEST")]
    public void Parse_IfInstruction_WithSimpleExpression_ParsesCorrectly(string input)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpIfInstruction>(result);

        var ifInst = (TpIfInstruction)result;
        Assert.NotNull(ifInst.Expression);
        Assert.NotNull(ifInst.Action);

        var expr = Assert.IsType<TpIfExpressionLogic>(ifInst.Expression);
        Assert.IsType<TpLogicExpressionSingle>(expr.Expression);
    }

    [Theory]
    [InlineData("IF R[1]>0 AND R[2]<10, JMP LBL[1]")]
    [InlineData("IF DI[1]=ON AND DI[2]=ON AND DI[3]=ON, CALL TEST")]
    public void Parse_IfInstruction_WithAndExpression_ParsesCorrectly(string input)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpIfInstruction>(result);

        var ifInst = (TpIfInstruction)result;
        Assert.NotNull(ifInst.Expression);
        Assert.NotNull(ifInst.Action);

        var expr = Assert.IsType<TpIfExpressionLogic>(ifInst.Expression);
        var andExpr = Assert.IsType<TpLogicExpressionAnd>(expr.Expression);
        Assert.NotEmpty(andExpr.Expression);
        Assert.True(andExpr.Expression.Count >= 2);
    }

    [Theory]
    [InlineData("IF R[1]>0 OR R[2]<10, JMP LBL[1]")]
    [InlineData("IF DI[1]=ON OR DI[2]=ON OR DI[3]=ON, CALL TEST")]
    public void Parse_IfInstruction_WithOrExpression_ParsesCorrectly(string input)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpIfInstruction>(result);

        var ifInst = (TpIfInstruction)result;
        Assert.NotNull(ifInst.Expression);
        Assert.NotNull(ifInst.Action);

        var expr = Assert.IsType<TpIfExpressionLogic>(ifInst.Expression);
        var orExpr = Assert.IsType<TpLogicExpressionOr>(expr.Expression);
        Assert.NotEmpty(orExpr.Expression);
        Assert.True(orExpr.Expression.Count >= 2);
    }

    [Theory]
    [InlineData("IF (DI[1] AND DI[2] OR DI[3]), JMP LBL[1]")]
    [InlineData("IF (R[1]>0 AND (R[2]=5 OR R[3]<10)), CALL TEST")]
    [InlineData("IF (R[132:OverrideTarget]>0),$MCR.$GENOVERRIDE=R[132:OverrideTarget]")]
    public void Parse_IfInstruction_WithMixedLogicExpression_ParsesCorrectly(string input)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpIfInstruction>(result);

        var ifInst = (TpIfInstruction)result;
        Assert.NotNull(ifInst.Expression);
        Assert.NotNull(ifInst.Action);

        Assert.IsType<TpIfExpressionMixedLogic>(ifInst.Expression);
    }

    [Theory]
    [InlineData("IF (DI[1]) THEN")]
    [InlineData("IF (R[1]>0 AND R[2]<10) THEN")]
    [InlineData("IF (DI[1] OR DO[2]) THEN")]
    [InlineData("IF (!DI[R[200]]) THEN")]
    [InlineData("IF ($[AGT]CONF.EXT_AXIS.COL_ROT.ENB=1) AND (DO[144:CommonApproach]=OFF) THEN")]
    public void Parse_IfThenInstruction_ParsesCorrectly(string input)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpIfThenInstruction>(result);

        var ifThen = (TpIfThenInstruction)result;
        Assert.NotNull(ifThen.Expression);
    }

    [Theory]
    [InlineData("ELSE")]
    public void Parse_ElseInstruction_ParsesCorrectly(string input)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpElseInstruction>(result);
    }

    [Theory]
    [InlineData("ENDIF")]
    public void Parse_EndIfInstruction_ParsesCorrectly(string input)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpEndIfInstruction>(result);
    }

    [Theory]
    [InlineData("SELECT R[1]=1, JMP LBL[1]")]
    [InlineData("SELECT R[10]=5, CALL TEST")]
    [InlineData("SELECT R[251:TAKE_Image_ID]=(-45),JMP LBL[9101] ")]
    public void Parse_SelectInstruction_ParsesCorrectly(string input)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpSelectInstruction>(result);

        var select = (TpSelectInstruction)result;
        Assert.NotNull(select.Register);
        Assert.NotNull(select.Case);

        Assert.IsType<TpValueIntegerConstant>(select.Case.Value);
        Assert.NotNull(select.Case.Action);
    }

    [Theory]
    [InlineData("=10, JMP LBL[1]")]
    [InlineData("=100, CALL TEST")]
    public void Parse_SelectCaseInstruction_WithValue_ParsesCorrectly(string input)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpSelectCaseInstruction>(result);

        var caseInst = (TpSelectCaseInstruction)result;
        Assert.NotNull(caseInst.SelectCase);
        Assert.NotNull(caseInst.SelectCase.Value);
        Assert.NotNull(caseInst.SelectCase.Action);

        Assert.IsType<TpValueIntegerConstant>(caseInst.SelectCase.Value);
    }

    [Theory]
    [InlineData("ELSE, JMP LBL[1]")]
    [InlineData("ELSE, CALL TEST")]
    public void Parse_SelectCaseInstruction_WithElse_ParsesCorrectly(string input)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpSelectCaseInstruction>(result);

        var caseInst = (TpSelectCaseInstruction)result;
        Assert.NotNull(caseInst.SelectCase);
        Assert.NotNull(caseInst.SelectCase.Action);

        Assert.IsType<TpSelectElseCase>(caseInst.SelectCase);
    }

    [Theory]
    [InlineData("LBL[")]
    [InlineData("JMP")]
    [InlineData("CALL")]
    [InlineData("IF")]
    [InlineData("IF R[1]>0")]
    [InlineData("IF R[1]>0,")]
    [InlineData("IF (R[1]>0)")]
    [InlineData("IF (R[1]>0) THE")]
    [InlineData("SELECT")]
    [InlineData("SELECT R[1]")]
    [InlineData("SELECT R[1]=")]
    public void Parse_InvalidBranchingInstruction_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpBranchingInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("IF (R[1]+5>10), JMP LBL[1]")]
    [InlineData("IF (R[1]*2=10), CALL TEST")]
    [InlineData("IF (R[1]+R[2]>R[3]-R[4]), JMP LBL[5]")]
    [InlineData("IF (R[1]*2>R[2]/2 AND R[3]+4<100), CALL PROGRAM_1")]
    [InlineData("IF ((R[1]+R[2])*3>(R[3]-R[4])*2), JMP LBL[10]")]
    public void Parse_IfInstruction_WithArithmeticExpression_ParsesCorrectly(string input)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpIfInstruction>(result);

        var ifInst = (TpIfInstruction)result;
        Assert.NotNull(ifInst.Expression);
        Assert.NotNull(ifInst.Action);

        Assert.IsType<TpIfExpressionMixedLogic>(ifInst.Expression);

        // Verify the instruction contains a TpMixedLogicBinaryArithmetic somewhere in its expression tree
        var mixedLogicExpr = (TpIfExpressionMixedLogic)ifInst.Expression;
        Assert.True(ContainsArithmeticOperation(mixedLogicExpr.Expression));
    }

    // Helper method to check if the expression tree contains arithmetic operations
    private static bool ContainsArithmeticOperation(TpMixedLogicExpression expression) =>
        expression switch
        {
            TpMixedLogicBinaryArithmetic => true,
            TpMixedLogicBinary binary => ContainsArithmeticOperation(binary.Lhs) ||
                                         ContainsArithmeticOperation(binary.Rhs),
            TpMixedLogicUnaryNot not => ContainsArithmeticOperation(not.Term),
            _ => false
        };
    [Theory]
    [InlineData("CALL TEST", "TEST")]
    [InlineData("CALL MAIN", "MAIN")]
    [InlineData("CALL PROGRAM_1", "PROGRAM_1")]
    [InlineData("CALL MY_ROUTINE", "MY_ROUTINE")]
    public void Parse_CallInstruction_WithoutArguments_ParsesCorrectly(string input, string expectedProgramName)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var call = Assert.IsType<TpCallInstruction>(result);
        var method = Assert.IsType<TpCallByName>(call.CallMethod);

        Assert.Equal(expectedProgramName, method.ProgramName);
        Assert.Empty(call.Arguments);
    }

    [Theory]
    [InlineData("CALL TEST(1)", "TEST", 1)]
    [InlineData("CALL TEST(1,2,3)", "TEST", 3)]
    [InlineData("CALL ROUTINE(R[1])", "ROUTINE", 1)]
    [InlineData("CALL PROGRAM(R[1],2,R[3])", "PROGRAM", 3)]
    [InlineData("CALL TEST(1,2,3,4,5,6,7,8,9,10)", "TEST", 10)]
    [InlineData("CALL TEST('STRING')", "TEST", 1)]
    [InlineData("CALL TEST(1.5)", "TEST", 1)]
    [InlineData("CALL TEST(ON)", "TEST", 1)]
    [InlineData("CALL AGT_TOUCH(2,AR[1],AR[2],AR[3],100)", "AGT_TOUCH", 5)]
    [InlineData("CALL SET_EXT_AXIS('ALL','PR95','PR62')", "SET_EXT_AXIS", 3)]
    public void Parse_CallInstruction_WithArguments_ParsesCorrectly(string input, string expectedProgramName, int expectedArgCount)
    {
        var result = TpBranchingInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpCallInstruction>(result);

        var call = Assert.IsType<TpCallInstruction>(result);
        var method = Assert.IsType<TpCallByName>(call.CallMethod);

        Assert.Equal(expectedProgramName, method.ProgramName);
        Assert.NotNull(call.Arguments);
        Assert.Equal(expectedArgCount, call.Arguments.Count);
        Assert.All(call.Arguments, arg => Assert.IsAssignableFrom<TpValue>(arg));
    }
}
