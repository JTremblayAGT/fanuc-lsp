using Sprache;

namespace TPLangParser.TPLang.Instructions;

public record TpProgramControlInstruction() : TpInstruction(0), ITpParser<TpProgramControlInstruction>
{
    public new static Parser<TpProgramControlInstruction> GetParser()
        => TpPauseInstruction.GetParser()
            .Or(TpAbortInstruction.GetParser())
            .Or(TpErrorProgramInstruction.GetParser())
            .Or(TpResumeProgramInstruction.GetParser())
            .Or(TpMaintenanceProgramInstruction.GetParser())
            .Or(TpClearResumeProgramInstruction.GetParser())
            .Or(TpReturnPathDisableInstruction.GetParser());
}

public sealed record TpPauseInstruction : TpProgramControlInstruction, ITpParser<TpProgramControlInstruction>
{
    public new static Parser<TpProgramControlInstruction> GetParser()
        => TpCommon.Keyword("PAUSE").Return(new TpPauseInstruction());
}

public sealed record TpAbortInstruction : TpProgramControlInstruction, ITpParser<TpProgramControlInstruction>
{
    public new static Parser<TpProgramControlInstruction> GetParser()
        => TpCommon.Keyword("ABORT").Return(new TpAbortInstruction());
}

public sealed record TpErrorProgramInstruction(string ProgramName)
    : TpProgramControlInstruction, ITpParser<TpProgramControlInstruction>
{
    public new static Parser<TpProgramControlInstruction> GetParser()
        => from keyword in TpCommon.Keyword("ERROR_PROG")
            from sep in TpCommon.Keyword("=")
            from programName in TpCommon.ProgramName
            select new TpErrorProgramInstruction(programName);
}

public sealed record TpResumeProgramInstruction(string ProgramName)
    : TpProgramControlInstruction, ITpParser<TpProgramControlInstruction>
{
    public new static Parser<TpProgramControlInstruction> GetParser()
        => from keyword in TpCommon.Keyword("RESUME_PROG")
            from sep in TpCommon.Keyword("=")
            from programName in TpCommon.ProgramName
            select new TpResumeProgramInstruction(programName);
}

public sealed record TpMaintenanceProgramInstruction(string ProgramName)
    : TpProgramControlInstruction, ITpParser<TpProgramControlInstruction>
{
    public new static Parser<TpProgramControlInstruction> GetParser()
        => from keyword in TpCommon.Keyword("MAINT_PROG")
            from sep in TpCommon.Keyword("=")
            from programName in TpCommon.ProgramName
            select new TpMaintenanceProgramInstruction(programName);
}

public sealed record TpClearResumeProgramInstruction : TpProgramControlInstruction, ITpParser<TpProgramControlInstruction>
{
    public new static Parser<TpProgramControlInstruction> GetParser()
        => TpCommon.Keyword("CLEAR_RESUME_PROG").Return(new TpClearResumeProgramInstruction());
}

public sealed record TpReturnPathDisableInstruction : TpProgramControlInstruction, ITpParser<TpProgramControlInstruction>
{
    public new static Parser<TpProgramControlInstruction> GetParser()
        => TpCommon.Keyword("RETURN_PATH_DSBL").Return(new TpReturnPathDisableInstruction());
}