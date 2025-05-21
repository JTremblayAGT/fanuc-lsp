using Sprache;

namespace AGT.TPLangParser.TPLang.Instructions;
public enum TpMotionType
{
    Joint,          // J
    Linear,         // L
    Circular,       // C
    CircularArc,    // A
    Spline          // S
}

public struct TpMotionTypeParser
{
    public static Parser<TpMotionType> Parser =
        TpCommon.Keyword("J").Return(TpMotionType.Joint)
            .Or(TpCommon.Keyword("L").Return(TpMotionType.Linear))
            .Or(TpCommon.Keyword("C").Return(TpMotionType.Circular))
            .Or(TpCommon.Keyword("A").Return(TpMotionType.CircularArc))
            .Or(TpCommon.Keyword("S").Return(TpMotionType.Spline));
}

public enum TpSpeedUnit
{
    Percentage,
    Seconds,
    InchPerMin,
    DegPerSec,
    MmPerSec,
    CmPerMin,
}

public abstract record TpMotionSpeed : ITpParser<TpMotionSpeed>
{
    public static readonly Parser<TpSpeedUnit> SpeedUnitParser =
        TpCommon.Keyword("%").Return(TpSpeedUnit.Percentage)
            .Or(TpCommon.Keyword("sec").Return(TpSpeedUnit.Seconds))
            .Or(TpCommon.Keyword("inch/min").Return(TpSpeedUnit.InchPerMin))
            .Or(TpCommon.Keyword("deg/sec").Return(TpSpeedUnit.DegPerSec))
            .Or(TpCommon.Keyword("mm/sec").Return(TpSpeedUnit.MmPerSec))
            .Or(TpCommon.Keyword("cm/min").Return(TpSpeedUnit.CmPerMin));
    public static Parser<TpMotionSpeed> GetParser()
        => TpMotionSpeedLiteral.GetParser()
            .Or(TpMotionSpeedIndirect.GetParser())
            .Or(TpMotionSpeedWeld.GetParser());
}

public sealed record TpMotionSpeedLiteral(TpSpeedUnit Unit, double Value) : TpMotionSpeed, ITpParser<TpMotionSpeed>
{
    public new static Parser<TpMotionSpeed> GetParser()
        => from value in Parse.Decimal.Select(double.Parse)
           from type in SpeedUnitParser
           select new TpMotionSpeedLiteral(type, value);

}

public sealed record TpMotionSpeedIndirect(TpRegister Register, TpSpeedUnit Unit) : TpMotionSpeed, ITpParser<TpMotionSpeed>
{
    public new static Parser<TpMotionSpeed> GetParser()
        => from register in TpRegister.GetParser().Or(TpArgumentRegister.GetParser())
           from unit in SpeedUnitParser.Optional()
           select new TpMotionSpeedIndirect(register, unit.GetOrElse(TpSpeedUnit.Percentage));
}

public sealed record TpMotionSpeedWeld : TpMotionSpeed, ITpParser<TpMotionSpeed>
{
    public new static Parser<TpMotionSpeed> GetParser()
        => TpCommon.Keyword("WELD_SPEED").Return(new TpMotionSpeedWeld());
}

public enum TpTerminationType
{
    Fine,
    Cnt,
    Cd
}

public sealed record TpMotionTermination(TpTerminationType Type, int? Value) : ITpParser<TpMotionTermination>
{
    public static Parser<TpMotionTermination> GetParser()
        => TpCommon.Keyword("FINE").Return(new TpMotionTermination(TpTerminationType.Fine, null))
            .Or(TpCommon.Keyword("CNT").Then(_ => Parse.Number.Select(int.Parse))
                .Select(val => new TpMotionTermination(TpTerminationType.Cnt, val)))
            .Or(TpCommon.Keyword("CD").Then(_ => Parse.Number.Select(int.Parse))
                .Select(val => new TpMotionTermination(TpTerminationType.Cd, val)));
}

public abstract record TpMotionOption : ITpParser<TpMotionOption>
{
    public static Parser<TpMotionOption> GetParser()
        => TpWristJointOption.GetParser()
            .Or(TpAccOption.GetParser())
            //.Or(TpAlimOption.GetParser()) TODO: ALIM goes BEFORE the termination type, see what to do about this
            .Or(TpPathOption.GetParser())
            .Or(TpLinearDistanceOption.GetParser())
            .Or(TpBreakOption.GetParser())
            .Or(TpOffsetOption.GetParser())
            .Or(TpToolOffsetOption.GetParser())
            .Or(TpOrntBaseOption.GetParser())
            .Or(TpRemoteTcpOption.GetParser())
            .Or(TpSkipJumpOption.GetParser())
            .Or(TpTimeBeforeOption.GetParser()) // TODO: Support POINT_LOGIC
            .Or(TpTimeAfterOption.GetParser()) // TODO: Support POINT_LOGIC
            .Or(TpDistanceBeforeOption.GetParser()) // TODO: Support POINT_LOGIC
            .Or(TpWeldOption.GetParser())
            .Or(TpTorchAngleOption.GetParser())
            //.Or(TpTouchSensingOption.GetParser())
            .Or(TpCoordMotionOption.GetParser())
            .Or(TpExtendedVelocityOption.GetParser())
            .Or(TpFaceplateLinearOption.GetParser())
            //.Or(TpCornerRegionOption.GetParser()) TODO: validate whether this needs to exist
            //.Or(TpVisionOffsetOption.GetParser())
            .Or(TpIncrementalOption.GetParser())
            .Or(TpSkipOption.GetParser());
    //.Or(TpSpotWeldOption.GetParser());
}

public sealed record TpWristJointOption : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => TpCommon.Keyword("Wjnt").Return(new TpWristJointOption());
}

public sealed record TpAccOption(int Value) : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => from keyword in TpCommon.Keyword("ACC")
           from value in Parse.Number.Select(int.Parse)
           select new TpAccOption(value);
}

public abstract record TpAlimOption(int GroupNumber) : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => TpAlimOptionLiteral.GetParser()
            .Or(TpAlimOptionRegister.GetParser());
}

public sealed record TpAlimOptionLiteral(int Accel, int GroupNumber)
    : TpAlimOption(GroupNumber), ITpParser<TpAlimOption>
{
    public new static Parser<TpAlimOption> GetParser()
        => from keyword in TpCommon.Keyword("ALIM")
           from lparen in Parse.Char('(')
           from value in Parse.Number.Select(int.Parse)
           from groupNumber in Parse.Char(',').Then(_ => Parse.Number.Select(int.Parse)).Optional()
           from rparen in Parse.Char('(')
           select new TpAlimOptionLiteral(value, groupNumber.GetOrElse(1));
}

public sealed record TpAlimOptionRegister(TpRegister Register, int GroupNumber)
    : TpAlimOption(GroupNumber), ITpParser<TpAlimOption>
{
    public new static Parser<TpAlimOption> GetParser()
        => from keyword in TpCommon.Keyword("ALIM")
           from lparen in Parse.Char('(')
           from register in TpRegister.GetParser()
           from groupNumber in Parse.Char(',').Then(_ => Parse.Number.Select(int.Parse)).Optional()
           from rparen in Parse.Char('(')
           select new TpAlimOptionRegister(register, groupNumber.GetOrElse(1));
}

public sealed record TpPathOption : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => TpCommon.Keyword("PTH").Return(new TpPathOption());
}

public enum TpLinearDistanceType
{
    Approach,
    Retract
}

public struct TpLinearDistanceTypeParser
{
    public static readonly Parser<TpLinearDistanceType> Parser =
        TpCommon.Keyword("AP_LD").Return(TpLinearDistanceType.Approach)
            .Or(TpCommon.Keyword("RT_LD").Return(TpLinearDistanceType.Retract))
            ;
}

public abstract record TpLinearDistanceOption(TpLinearDistanceType Type) : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => TpLinearDistanceOptionLiteral.GetParser()
            .Or(TpLinearDistanceOptionRegister.GetParser());
}

public sealed record TpLinearDistanceOptionLiteral(TpLinearDistanceType Type, int Distance)
    : TpLinearDistanceOption(Type), ITpParser<TpLinearDistanceOption>
{
    public new static Parser<TpLinearDistanceOption> GetParser()
        => from type in TpLinearDistanceTypeParser.Parser
           from distance in Parse.Number.Select(int.Parse)
           select new TpLinearDistanceOptionLiteral(type, distance);
}

public sealed record TpLinearDistanceOptionRegister(TpLinearDistanceType Type, TpRegister Register)
    : TpLinearDistanceOption(Type), ITpParser<TpLinearDistanceOption>
{
    public new static Parser<TpLinearDistanceOption> GetParser()
        => from type in TpLinearDistanceTypeParser.Parser
           from register in TpRegister.GetParser()
           select new TpLinearDistanceOptionRegister(type, register);
}

public sealed record TpBreakOption : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => TpCommon.Keyword("BREAK").Return(new TpBreakOption());
}

public sealed record TpOffsetOption(TpPositionRegister? PositionRegister) : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => from keyword in TpCommon.Keyword("Offset")
           from posReg in Parse.Char(',')
               .Then(_ => Parse.WhiteSpace.Many())
               .Then(_ => TpPositionRegister.GetParser()).Optional()
           select new TpOffsetOption(posReg.GetOrElse(null));
}

public sealed record TpToolOffsetOption(TpPositionRegister? PositionRegister) : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => from keyword in TpCommon.Keyword("Tool_Offset")
           from posReg in Parse.Char(',')
               .Then(_ => Parse.WhiteSpace.Many())
               .Then(_ => TpPositionRegister.GetParser()).Optional()
           select new TpToolOffsetOption(posReg.GetOrElse(null));
}

public enum TpOrntBaseRefFrame
{
    WorldFrame,
    UserFrame,
    LeaderReferenceFrame
}

public struct TpOrntBaseRefFrameParser
{
    public static readonly Parser<TpOrntBaseRefFrame> Parser =
        TpCommon.Keyword("UF").Return(TpOrntBaseRefFrame.UserFrame)
            .Or(TpCommon.Keyword("LDR").Return(TpOrntBaseRefFrame.LeaderReferenceFrame));
}

public sealed record TpOrntBaseOption(TpOrntBaseRefFrame ReferenceFrame, int FrameIndex, char DirectionIndex) : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => from keyword in TpCommon.Keyword("ORNT_BASE")
           from option in (
               from refFrame in TpOrntBaseRefFrameParser.Parser
               from lbracket in Parse.Char('[')
               from frameIndex in Parse.Number.Select(int.Parse)
               from sep in Parse.Char(',')
               from dirIndex in Parse.Lower
               from rbracket in Parse.Char(']')
               select new TpOrntBaseOption(refFrame, frameIndex, dirIndex)
           ).Optional()
           select option.GetOrElse(new(TpOrntBaseRefFrame.WorldFrame, 0, 'z'));
}

public sealed record TpRemoteTcpOption : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => TpCommon.Keyword("RTCP").Return(new TpRemoteTcpOption());
}

public sealed record TpSkipJumpOption(TpLabel Label) : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => from keyword in TpCommon.Keyword("SkipJump,")
           from label in TpLabel.GetParser()
           select new TpSkipJumpOption(label);
}

public sealed record TpTimeBeforeOption(double Time, string Program) : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => from keyword in TpCommon.Keyword("TIME BEFORE")
           from time in Parse.Decimal.Select(double.Parse)
           from sec in TpCommon.Keyword("sec,")
           from callToken in TpCommon.Keyword("CALL")
           from program in TpCommon.ProgramName
           select new TpTimeBeforeOption(time, program);
}

public sealed record TpTimeAfterOption(double Time, string Program) : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => from keyword in TpCommon.Keyword("TIME AFTER")
           from time in Parse.Decimal.Select(double.Parse)
           from sec in TpCommon.Keyword("sec,")
           from callToken in TpCommon.Keyword("CALL")
           from program in Parse.LetterOrDigit.Or(Parse.Char('_')).AtLeastOnce().Text()
           select new TpTimeAfterOption(time, program);
}

public sealed record TpDistanceBeforeOption(double Distance, string Program) : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => from keyword in TpCommon.Keyword("DISTANCE BEFORE")
           from time in Parse.Decimal.Select(double.Parse)
           from sec in TpCommon.Keyword("mm")
           from callToken in TpCommon.Keyword("CALL")
           from program in Parse.LetterOrDigit.Or(Parse.Char('_')).AtLeastOnce().Text()
           select new TpDistanceBeforeOption(time, program);
}


public abstract record TpWeldOptionArguments : ITpParser<TpWeldOptionArguments>
{
    public static Parser<TpWeldOptionArguments> GetParser()
        => TpWeldOptionProcedures.GetParser()
            .Or(TpWeldOptionParameters.GetParser());
}
public abstract record TpWeldOptionArg : ITpParser<TpWeldOptionArg>
{
    public static Parser<TpWeldOptionArg> GetParser()
        => TpWeldOptionScheduleArg.GetParser()
            .Or(TpWeldOptionRegisterArg.GetParser())
            ;
}

public sealed record TpWeldOptionScheduleArg(int ScheduleNumber) : TpWeldOptionArg, ITpParser<TpWeldOptionArg>
{
    public new static Parser<TpWeldOptionArg> GetParser()
        => Parse.Number.Select(num => new TpWeldOptionScheduleArg(int.Parse(num)));
}

public sealed record TpWeldOptionRegisterArg(TpRegister Register) : TpWeldOptionArg, ITpParser<TpWeldOptionArg>
{
    public new static Parser<TpWeldOptionArg> GetParser()
        => TpRegister.GetParser().Select(reg => new TpWeldOptionRegisterArg(reg));
}

public sealed record TpWeldOptionProcedures(TpWeldOptionArg Procedure, TpWeldOptionArg Schedule) : TpWeldOptionArguments, ITpParser<TpWeldOptionProcedures>
{
    public new static Parser<TpWeldOptionProcedures> GetParser()
        => from lbracket in Parse.Char('[')
           from procedure in TpWeldOptionArg.GetParser()
           from sep in Parse.Char(',')
           from schedule in TpWeldOptionArg.GetParser()
           from rbracket in Parse.Char(']')
           select new TpWeldOptionProcedures(procedure, schedule);
}

public sealed record TpWeldOptionParameters(List<string> Parameters)
    : TpWeldOptionArguments, ITpParser<TpWeldOptionArguments>
{
    public new static Parser<TpWeldOptionArguments> GetParser()
        => from lbracket in Parse.Char('[')
           from arguments in
               Parse.LetterOrDigit.Or(Parse.Char('.')).AtLeastOnce().Text()
                   .DelimitedBy(Parse.Char(','))
           from rbracket in Parse.Char(']')
           select new TpWeldOptionParameters(arguments.ToList());

}

public sealed record TpWeldOption(TpArcWeldingOptionType Type, TpWeldOptionArguments Args, int WeldEquipment)
    : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => from keyword in TpCommon.Keyword("Arc")
           from type in TpArcWeldingOptionTypeParser.Parser
           from weldEquipment in Parse.Char('E').Then(_ => Parse.Number.Select(int.Parse)).Optional()
           from args in TpWeldOptionArguments.GetParser()
           select new TpWeldOption(type, args, weldEquipment.GetOrElse(1));
}


public sealed record TpTorchAngleOption(TpPositionRegister? PositionRegister) : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => from keyword in TpCommon.Keyword("TA_REF")
           from posReg in TpPositionRegister.GetParser().Optional()
           select new TpTorchAngleOption(posReg.GetOrElse(null));
}

public sealed record TpCoordMotionOption() : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => TpCommon.Keyword("COORD").Return(new TpCoordMotionOption());
}

public sealed record TpExtendedVelocityOption(int Value, bool IsIndependent) : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => from whitespace in Parse.WhiteSpace.Many()
           from isIndependent in TpCommon.Keyword("Ind.").Optional()
               .Select(opt => opt.IsDefined)
           from keyword in TpCommon.Keyword("EV")
           from value in Parse.Number.Select(int.Parse)
           from tail in Parse.Char('%')
           select new TpExtendedVelocityOption(value, isIndependent);
}

public sealed record TpFaceplateLinearOption : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => TpCommon.Keyword("FPLIN").Return(new TpFaceplateLinearOption());
}


public sealed record TpIncrementalOption() : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => TpCommon.Keyword("INC").Return(new TpIncrementalOption());
}

public sealed record TpSkipOption(TpLabel Label, TpPosRegAssignmentInstruction? Assignment) : TpMotionOption, ITpParser<TpMotionOption>
{
    public new static Parser<TpMotionOption> GetParser()
        => from keyword in TpCommon.Keyword("Skip,")
           from label in TpLabel.GetParser()
           from assn in (from sep in TpCommon.Keyword(",")
                         from assign in TpPosRegAssignmentInstruction.GetParser()
                         select assign as TpPosRegAssignmentInstruction).Optional()
           select new TpSkipOption(label, assn.GetOrElse(null));
}

public sealed record TpMotionInstruction(
    TpMotionType MotionType,
    List<TpPosition> Positions,
    TpMotionSpeed Speed,
    TpMotionTermination Termination,
    List<TpMotionOption> Options)
: TpInstruction(0), ITpParser<TpMotionInstruction>
{
    public new static Parser<TpMotionInstruction> GetParser()
        => from motionType in TpMotionTypeParser.Parser
           from position in TpPosition.GetParser().AtLeastOnce()
               .Select(lst => lst.ToList())
           from speed in TpMotionSpeed.GetParser()
           from termination in TpMotionTermination.GetParser()
           from options in TpMotionOption.GetParser().Many()
           select new TpMotionInstruction(motionType, position, speed, termination,
               options.ToList());

    public TpPosition PrimaryPosition => Positions.First();

    public TpPosition SecondaryPosition =>
        MotionType switch
        {
            TpMotionType.Circular or TpMotionType.CircularArc => Positions.Count == 2
                ? Positions.Last()
                : throw new InvalidOperationException(
                    $"A motion instruction of type [{MotionType}] should have two positions"),
            _ => throw new InvalidOperationException("Only Circular and Circular Arc motions have two positions.")
        };
}
