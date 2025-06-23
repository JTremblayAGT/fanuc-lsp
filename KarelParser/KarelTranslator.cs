using ParserUtils;
using Sprache;

namespace KarelParser;

public record KarelTranslatorDirective
    : WithPosition, IKarelParser<KarelTranslatorDirective>
{
    protected const char Leader = '%';

    protected static Parser<string> Keyword(string keyword)
        => Parse.Char(Leader).Then(_ => KarelCommon.Keyword(keyword));

    private static Parser<KarelTranslatorDirective> InternalParser()
        => KarelAlphabetizeDirective.GetParser()
            .Or(KarelCmosVarsDirective.GetParser())
            .Or(KarelCmosShadowDirective.GetParser())
            .Or(KarelCommentDirective.GetParser())
            .Or(KarelCrtDeviceDirective.GetParser())
            .Or(KarelDefaultGroupDirective.GetParser())
            .Or(KarelDelayDirective.GetParser())
            .Or(KarelEnvironmentDirective.GetParser())
            .Or(KarelIncludeDirective.GetParser())
            .Or(KarelLockGroupDirective.GetParser())
            .Or(KarelNoAbortDirective.GetParser())
            .Or(KarelNoBusyLampDirective.GetParser())
            .Or(KarelNoLockGroupDirective.GetParser())
            .Or(KarelNoPauseDirective.GetParser())
            .Or(KarelNoPauseShiftDirective.GetParser())
            .Or(KarelPriorityDirective.GetParser())
            .Or(KarelShadowVarsDirective.GetParser())
            .Or(KarelStackSizeDirective.GetParser())
            .Or(KarelTimeSliceDirective.GetParser())
            .Or(KarelTpMotionDirective.GetParser())
            .Or(KarelUninitVarsDirective.GetParser())
            .Or(KarelSystemDirective.GetParser());

    public static Parser<KarelTranslatorDirective> GetParser()
        => InternalParser()
            .WithPosition()
            .Select(result => result.Value with
            {
                Start = result.Start,
                End = result.End
            });
}

public sealed record KarelAlphabetizeDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Keyword("ALPHABETIZE").Return(new KarelAlphabetizeDirective());
}

public sealed record KarelCmosVarsDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Keyword("CMOSVARS").Return(new KarelCmosVarsDirective());
}

public sealed record KarelCmosShadowDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Keyword("CMOS2SHADOW").Return(new KarelCmosShadowDirective());
}

public sealed record KarelCommentDirective(KarelString Comment)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Keyword("COMMENT")
           from sep in KarelCommon.Keyword("=")
           from cmt in KarelString.GetParser()
           select new KarelCommentDirective((KarelString)cmt);
}

public sealed record KarelCrtDeviceDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Keyword("CRTDEVICE").Return(new KarelCrtDeviceDirective());
}

public sealed record KarelDefaultGroupDirective(KarelInteger GroupNumber)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Keyword("DEFGROUP")
           from sep in KarelCommon.Keyword("=")
           from num in KarelInteger.GetParser()
           select new KarelDefaultGroupDirective((KarelInteger)num);
}

public sealed record KarelDelayDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Keyword("DELAY").Return(new KarelCrtDeviceDirective());
}

public sealed record KarelEnvironmentDirective(string FileName)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kv in Keyword("ENVIRONMENT")
           from fileName in KarelCommon.Identifier
           select new KarelEnvironmentDirective(fileName);
}

public sealed record KarelIncludeDirective(string FileName)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kv in Keyword("INCLUDE")
           from fileName in KarelCommon.Identifier
           select new KarelIncludeDirective(fileName);
}

public sealed record KarelLockGroupDirective(List<KarelInteger> LockedGroups)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Keyword("LOCKGROUP")
           from sep in KarelCommon.Keyword("=")
           from groups in KarelInteger.GetParser().DelimitedBy(Parse.Char(','))
           select new KarelLockGroupDirective(groups.OfType<KarelInteger>().ToList());
}

public sealed record KarelNoAbortDirective(List<string> Options)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Keyword("NOABORT")
           from sep in KarelCommon.Keyword("=")
           from options in KarelCommon.Reserved.DelimitedBy(KarelCommon.Keyword("+"))
           select new KarelNoAbortDirective(options.ToList());
}

public sealed record KarelNoBusyLampDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Keyword("NOBUSYLAMP").Return(new KarelNoBusyLampDirective());
}

public sealed record KarelNoLockGroupDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Keyword("NOLOCKGROUP").Return(new KarelNoLockGroupDirective());
}

public sealed record KarelNoPauseDirective(List<string> Options)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Keyword("NOPAUSE")
           from sep in KarelCommon.Keyword("=")
           from options in KarelCommon.Reserved.DelimitedBy(KarelCommon.Keyword("+"))
           select new KarelNoPauseDirective(options.ToList());
}

public sealed record KarelNoPauseShiftDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Keyword("NOPAUSESHFT").Return(new KarelNoPauseShiftDirective());
}

public sealed record KarelPriorityDirective(KarelInteger GroupNumber)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Keyword("PRIORITY")
           from sep in KarelCommon.Keyword("=")
           from num in KarelInteger.GetParser()
           select new KarelPriorityDirective((KarelInteger)num);
}

public sealed record KarelShadowVarsDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Keyword("SHADOWVARS").Return(new KarelShadowVarsDirective());
}

public sealed record KarelStackSizeDirective(KarelInteger GroupNumber)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Keyword("STACKSIZE")
           from sep in KarelCommon.Keyword("=")
           from num in KarelInteger.GetParser()
           select new KarelStackSizeDirective((KarelInteger)num);
}

public sealed record KarelTimeSliceDirective(KarelInteger GroupNumber)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Keyword("TIMESLICE")
           from sep in KarelCommon.Keyword("=")
           from num in KarelInteger.GetParser()
           select new KarelTimeSliceDirective((KarelInteger)num);
}

public sealed record KarelTpMotionDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Keyword("TPMOTION").Return(new KarelTpMotionDirective());
}

public sealed record KarelUninitVarsDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Keyword("UNINITVARS").Return(new KarelUninitVarsDirective());
}
public sealed record KarelSystemDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Keyword("SYSTEM").Return(new KarelSystemDirective());
}
