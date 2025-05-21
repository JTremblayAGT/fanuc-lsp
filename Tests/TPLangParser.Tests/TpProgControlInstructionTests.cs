using AGT.TPLangParser.TPLang.Instructions;
using Sprache;

namespace AGT.TPLangParser.Tests;
public class TpProgControlInstructionTests
{
    [Theory]
    [InlineData("PAUSE")]
    public void Parse_PauseInstruction_ParsesCorrectly(string input)
    {
        var result = TpProgramControlInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpPauseInstruction>(result);
    }

    [Theory]
    [InlineData("ABORT")]
    public void Parse_AbortInstruction_ParsesCorrectly(string input)
    {
        var result = TpProgramControlInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpAbortInstruction>(result);
    }

    [Theory]
    [InlineData("ERROR_PROG=HANDLER", "HANDLER")]
    [InlineData("ERROR_PROG=ERROR_ROUTINE", "ERROR_ROUTINE")]
    [InlineData("ERROR_PROG=ERR_PROG", "ERR_PROG")]
    public void Parse_ErrorProgramInstruction_ParsesCorrectly(string input, string expectedProgram)
    {
        var result = TpProgramControlInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var errorProgram = Assert.IsType<TpErrorProgramInstruction>(result);
        Assert.Equal(expectedProgram, errorProgram.ProgramName);
    }

    [Theory]
    [InlineData("RESUME_PROG=CONTINUE", "CONTINUE")]
    [InlineData("RESUME_PROG=RESUME_ROUTINE", "RESUME_ROUTINE")]
    [InlineData("RESUME_PROG=RES_PROG", "RES_PROG")]
    public void Parse_ResumeProgramInstruction_ParsesCorrectly(string input, string expectedProgram)
    {
        var result = TpProgramControlInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var resumeProgram = Assert.IsType<TpResumeProgramInstruction>(result);
        Assert.Equal(expectedProgram, resumeProgram.ProgramName);
    }

    [Theory]
    [InlineData("MAINT_PROG=MAINTENANCE", "MAINTENANCE")]
    [InlineData("MAINT_PROG=MAINT_ROUTINE", "MAINT_ROUTINE")]
    [InlineData("MAINT_PROG=MAINT_CODE", "MAINT_CODE")]
    public void Parse_MaintenanceProgramInstruction_ParsesCorrectly(string input, string expectedProgram)
    {
        var result = TpProgramControlInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        var maintProgram = Assert.IsType<TpMaintenanceProgramInstruction>(result);
        Assert.Equal(expectedProgram, maintProgram.ProgramName);
    }

    [Theory]
    [InlineData("CLEAR_RESUME_PROG")]
    public void Parse_ClearResumeProgramInstruction_ParsesCorrectly(string input)
    {
        var result = TpProgramControlInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpClearResumeProgramInstruction>(result);
    }

    [Theory]
    [InlineData("RETURN_PATH_DSBL")]
    public void Parse_ReturnPathDisableInstruction_ParsesCorrectly(string input)
    {
        var result = TpProgramControlInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpReturnPathDisableInstruction>(result);
    }

    [Theory]
    [InlineData("PAUS")]  // Misspelled keyword
    [InlineData("ABRT")]  // Misspelled keyword
    [InlineData("ERROR_PRG=HANDLER")]  // Incorrect keyword
    [InlineData("RESUME_PRG=CONTINUE")]  // Incorrect keyword
    [InlineData("MAINT_PRG=MAINTENANCE")]  // Incorrect keyword
    [InlineData("CLEAR_RESUMEPROG")]  // Incorrect keyword
    [InlineData("RETURNPATH_DSBL")]  // Incorrect keyword
    public void Parse_InvalidProgramControlInstruction_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpProgramControlInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("ERROR_PROG=")]  // Missing program name
    [InlineData("RESUME_PROG=")]  // Missing program name
    [InlineData("MAINT_PROG=")]  // Missing program name
    public void Parse_ProgramNameInstructions_WithoutProgramName_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpProgramControlInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("ERROR_PROG = HANDLER")]  // Spaces around equals
    [InlineData("RESUME_PROG = CONTINUE")]  // Spaces around equals
    [InlineData("MAINT_PROG = MAINTENANCE")]  // Spaces around equals
    public void Parse_ProgramControlInstruction_HandlesWhitespaceCorrectly(string input)
    {
        var result = TpProgramControlInstruction.GetParser().Parse(input);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("pause")]  // Lowercase command
    [InlineData("Abort")]  // Mixed case command
    [InlineData("error_prog=HANDLER")]  // Lowercase keyword
    public void Parse_ProgramControlInstruction_CaseSensitive_ThrowsParseException(string input)
        => Assert.Throws<ParseException>(() => TpProgramControlInstruction.GetParser().Parse(input));

    [Theory]
    [InlineData("ERROR_PROG=HANDLER1")]
    [InlineData("ERROR_PROG=ERROR_123")]
    [InlineData("ERROR_PROG=E_R1")]
    public void Parse_ErrorProgramInstruction_WithNumericProgramName_ParsesCorrectly(string input)
    {
        var result = TpProgramControlInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpErrorProgramInstruction>(result);
    }

    [Theory]
    [InlineData("RESUME_PROG=RESUME1")]
    [InlineData("RESUME_PROG=R_123")]
    public void Parse_ResumeProgramInstruction_WithNumericProgramName_ParsesCorrectly(string input)
    {
        var result = TpProgramControlInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpResumeProgramInstruction>(result);
    }

    [Theory]
    [InlineData("MAINT_PROG=MAINT1")]
    [InlineData("MAINT_PROG=M_123")]
    public void Parse_MaintenanceProgramInstruction_WithNumericProgramName_ParsesCorrectly(string input)
    {
        var result = TpProgramControlInstruction.GetParser().Parse(input);

        Assert.NotNull(result);
        Assert.IsType<TpMaintenanceProgramInstruction>(result);
    }
}
