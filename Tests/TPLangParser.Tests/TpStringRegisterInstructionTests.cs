using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;
using Sprache;

namespace TPLangParser.Tests;
public class TpStringRegisterInstructionTests
{
    [Theory]
    [InlineData("SR[1]=R[1]")]
    [InlineData("SR[10]=SR[5]")]
    [InlineData("SR[25]=AR[3]")]
    public void Parse_StringRegisterAssignment_ParsesCorrectly(string input)
    {
        var result = TpStringRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var assignment = Assert.IsType<TpStringRegisterAssignment>(result);
        Assert.NotNull(assignment.StringRegister);
        Assert.NotNull(assignment.Value);
    }

    [Theory]
    [InlineData("SR[1]=SR[2]+SR[3]")]
    [InlineData("SR[10]=R[5]+SR[3]")]
    [InlineData("SR[25]=SR[1]+R[2]")]
    public void Parse_StringRegisterConcatenation_ParsesCorrectly(string input)
    {
        var result = TpStringRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var concatenation = Assert.IsType<TpStringRegisterConcatenation>(result);
        Assert.NotNull(concatenation.StringRegister);
        Assert.NotNull(concatenation.Lhs);
        Assert.NotNull(concatenation.Rhs);
    }

    [Theory]
    [InlineData("R[1]=STRLEN SR[1]")]
    [InlineData("R[10]=STRLEN SR[5]")]
    [InlineData("R[25]=STRLEN SR[3]")]
    public void Parse_StringRegisterLength_ParsesCorrectly(string input)
    {
        var result = TpStringRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var length = Assert.IsType<TpStringRegisterLength>(result);
        Assert.NotNull(length.ResultRegister);
        Assert.NotNull(length.StringRegister);
    }

    [Theory]
    [InlineData("R[1]=FINDSTR SR[1],SR[2]")]
    [InlineData("R[10]=FINDSTR SR[5],AR[3]")]
    [InlineData("R[25]=FINDSTR AR[1],SR[3]")]
    public void Parse_StringRegisterSearch_ParsesCorrectly(string input)
    {
        var result = TpStringRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var search = Assert.IsType<TpStringRegisterSearch>(result);
        Assert.NotNull(search.ResultRegister);
        Assert.NotNull(search.InputString);
        Assert.NotNull(search.SearchString);
    }

    [Theory]
    [InlineData("SR[1]=SUBSTR SR[2],1,5")]
    [InlineData("SR[10]=SUBSTR SR[5],R[1],10")]
    [InlineData("SR[25]=SUBSTR AR[1],R[2],R[3]")]
    public void Parse_StringRegisterCut_ParsesCorrectly(string input)
    {
        var result = TpStringRegisterInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var cut = Assert.IsType<TpStringRegisterCut>(result);
        Assert.NotNull(cut.ResultRegister);
        Assert.NotNull(cut.InputString);
        Assert.NotNull(cut.BeginIndex);
        Assert.NotNull(cut.EndIndex);
    }

    [Theory]
    [InlineData("SR[1]=SR[2]", 1)]
    [InlineData("SR[10]=R[5]", 10)]
    [InlineData("SR[25]=AR[3]", 25)]
    public void Parse_StringRegisterAssignment_VerifyRegisterNumber(string input, int expectedNumber)
    {
        var result = TpStringRegisterInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpStringRegisterAssignment>(result);
        var access = Assert.IsType<TpAccessDirect>(assignment.StringRegister.Access);
        Assert.Equal(expectedNumber, access.Number);
    }

    [Theory]
    [InlineData("SR[1]=SR[2]+SR[3]", 2, 3)]
    [InlineData("SR[10]=R[5]+SR[3]", 5, 3)]
    public void Parse_StringRegisterConcatenation_VerifySourceRegisters(string input, int expectedLhs, int expectedRhs)
    {
        var result = TpStringRegisterInstruction.GetParser().Parse(input);

        var concatenation = Assert.IsType<TpStringRegisterConcatenation>(result);
        var lhsRegister = concatenation.Lhs.Register;
        var rhsRegister = concatenation.Rhs.Register;

        var lhsAccess = Assert.IsType<TpAccessDirect>(lhsRegister.Access);
        var rhsAccess = Assert.IsType<TpAccessDirect>(rhsRegister.Access);

        Assert.Equal(expectedLhs, lhsAccess.Number);
        Assert.Equal(expectedRhs, rhsAccess.Number);
    }

    [Theory]
    [InlineData("R[1]=STRLEN SR[2]", 1, 2)]
    [InlineData("R[10]=STRLEN SR[5]", 10, 5)]
    public void Parse_StringRegisterLength_VerifyRegisters(string input, int expectedResult, int expectedString)
    {
        var result = TpStringRegisterInstruction.GetParser().Parse(input);

        var length = Assert.IsType<TpStringRegisterLength>(result);
        var resultAccess = Assert.IsType<TpAccessDirect>(length.ResultRegister.Access);
        var stringAccess = Assert.IsType<TpAccessDirect>(length.StringRegister.Access);

        Assert.Equal(expectedResult, resultAccess.Number);
        Assert.Equal(expectedString, stringAccess.Number);
    }

    [Theory]
    [InlineData("R[1]=FINDSTR SR[2],SR[3]", 1, 2, 3)]
    [InlineData("R[10]=FINDSTR SR[5],AR[3]", 10, 5, 3)]
    public void Parse_StringRegisterSearch_VerifyRegisters(string input, int expectedResult, int expectedInput, int expectedSearch)
    {
        var result = TpStringRegisterInstruction.GetParser().Parse(input);

        var search = Assert.IsType<TpStringRegisterSearch>(result);
        var resultAccess = Assert.IsType<TpAccessDirect>(search.ResultRegister.Access);
        var inputAccess = Assert.IsType<TpAccessDirect>(search.InputString.Register.Access);
        var searchAccess = Assert.IsType<TpAccessDirect>(search.SearchString.Register.Access);

        Assert.Equal(expectedResult, resultAccess.Number);
        Assert.Equal(expectedInput, inputAccess.Number);
        Assert.Equal(expectedSearch, searchAccess.Number);
    }

    [Theory]
    [InlineData("SR[1]=SUBSTR SR[2],3,5", 1, 2, 3, 5)]
    public void Parse_StringRegisterCut_VerifyRegistersAndConstants(string input, int expectedResult, int expectedInput, int expectedBegin, int expectedEnd)
    {
        var result = TpStringRegisterInstruction.GetParser().Parse(input);

        var cut = Assert.IsType<TpStringRegisterCut>(result);
        var resultAccess = Assert.IsType<TpAccessDirect>(cut.ResultRegister.Access);
        var inputAccess = Assert.IsType<TpAccessDirect>(cut.InputString.Register.Access);

        Assert.Equal(expectedResult, resultAccess.Number);
        Assert.Equal(expectedInput, inputAccess.Number);

        var beginValue = Assert.IsType<TpValueIntegerConstant>(cut.BeginIndex);
        var endValue = Assert.IsType<TpValueIntegerConstant>(cut.EndIndex);

        Assert.Equal(expectedBegin, beginValue.Value);
        Assert.Equal(expectedEnd, endValue.Value);
    }

    [Theory]
    [InlineData("SR[1]=SUBSTR SR[2],R[3],R[5]")]
    public void Parse_StringRegisterCut_WithRegisterIndices_ParsesCorrectly(string input)
    {
        var result = TpStringRegisterInstruction.GetParser().Parse(input);

        var cut = Assert.IsType<TpStringRegisterCut>(result);
        Assert.IsType<TpValueRegister>(cut.BeginIndex);
        Assert.IsType<TpValueRegister>(cut.EndIndex);
    }

    [Theory]
    [InlineData("SR[1:Comment]=SR[2]", 1, "Comment")]
    [InlineData("SR[5:Test String]=R[3]", 5, "Test String")]
    public void Parse_StringRegisterAssignment_WithComment_ParsesCorrectly(string input, int expectedNumber, string expectedComment)
    {
        var result = TpStringRegisterInstruction.GetParser().Parse(input);

        var assignment = Assert.IsType<TpStringRegisterAssignment>(result);
        var access = Assert.IsType<TpAccessDirect>(assignment.StringRegister.Access);
        Assert.Equal(expectedNumber, access.Number);
        Assert.Equal(expectedComment, access.Comment);
    }

    [Theory]
    [InlineData("SR[1]=")] // Missing expression
    [InlineData("SR=R[1]")] // Invalid SR format
    [InlineData("SR[]=")] // Missing register number
    [InlineData("R[1]=STRLEN")] // Missing register for STRLEN
    [InlineData("R[1]=FINDSTR SR[1]")] // Missing second argument for FINDSTR
    [InlineData("SR[1]=SUBSTR SR[2],1")] // Missing end index for SUBSTR
    public void Parse_InvalidStringRegisterInstruction_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpStringRegisterInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("SR[1] = SR[2]")]  // Spaces around equals
    [InlineData("SR[ 1 ] = R[ 2 ]")]  // Spaces inside brackets
    [InlineData("SR[1] = SR[2] + SR[3]")]  // Spaces around operator
    [InlineData("R[1] = STRLEN SR[2]")]  // Spaces around STRLEN
    [InlineData("R[1] = FINDSTR SR[2], SR[3]")]  // Spaces around comma
    [InlineData("SR[1] = SUBSTR SR[2], 1, 5")]  // Spaces in SUBSTR
    public void Parse_StringRegisterInstruction_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpStringRegisterInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("sr[1]=SR[2]")]  // Lowercase sr
    [InlineData("SR[1]=sr[2]")]  // Lowercase sr in value
    [InlineData("R[1]=strlen SR[2]")]  // Lowercase function name
    [InlineData("R[1]=FINDSTR sr[2],SR[3]")]  // Lowercase sr in argument
    public void Parse_StringRegisterInstruction_CaseSensitive_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpStringRegisterInstruction.GetParser().Parse(input));
}
