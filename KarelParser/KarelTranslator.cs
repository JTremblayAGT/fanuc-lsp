using ParserUtils;
using Sprache;

namespace KarelParser;

public record KarelTranslatorDirective
    : WithPosition, IKarelParser<KarelTranslatorDirective>
{
    protected const string Leader = "%";

    protected static Parser<string> Directive(string keyword)
        => KarelCommon.Keyword(Leader).Then(_ => KarelCommon.Keyword(keyword)
            .Or(KarelCommon.Reserved.Then(dir => dir.Equals(keyword)
            ? Parse.Return(dir)
            : i => Result.Failure<string>(i,
                $"'{keyword}' isn't a valid translator directive",
                ["%DIRECTIVE"]))));

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
            .Or(KarelNoPauseShiftDirective.GetParser())
            .Or(KarelNoPauseDirective.GetParser())
            .Or(KarelPriorityDirective.GetParser())
            .Or(KarelShadowVarsDirective.GetParser())
            .Or(KarelStackSizeDirective.GetParser())
            .Or(KarelTimeSliceDirective.GetParser())
            .Or(KarelTpMotionDirective.GetParser())
            .Or(KarelUninitVarsDirective.GetParser())
            .Or(KarelSystemDirective.GetParser());

    public static Parser<KarelTranslatorDirective> GetParser()
        => InternalParser().WithPos();
}

public sealed record KarelAlphabetizeDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Directive("ALPHABETIZE").Return(new KarelAlphabetizeDirective());
}

public sealed record KarelCmosVarsDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Directive("CMOSVARS").Return(new KarelCmosVarsDirective());
}

public sealed record KarelCmosShadowDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Directive("CMOS2SHADOW").Return(new KarelCmosShadowDirective());
}

public sealed record KarelCommentDirective(KarelString Comment)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Directive("COMMENT")
           from sep in KarelCommon.Keyword("=")
           from cmt in KarelString.GetParser()
           select new KarelCommentDirective((KarelString)cmt);
}

public sealed record KarelCrtDeviceDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Directive("CRTDEVICE").Return(new KarelCrtDeviceDirective());
}

public sealed record KarelDefaultGroupDirective(KarelInteger GroupNumber)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Directive("DEFGROUP")
           from sep in KarelCommon.Keyword("=")
           from num in KarelInteger.GetParser()
           select new KarelDefaultGroupDirective((KarelInteger)num);
}

public sealed record KarelDelayDirective(KarelInteger Value)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Directive("DELAY")
           from sep in KarelCommon.Keyword("=")
           from num in KarelInteger.GetParser()
           select new KarelDelayDirective((KarelInteger)num);
}

public sealed record KarelEnvironmentDirective(string FileName)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kv in Directive("ENVIRONMENT")
           from fileName in KarelCommon.Identifier
           select new KarelEnvironmentDirective(fileName);
}

public sealed record KarelIncludeDirective(string FileName, Uri Uri, List<KarelDeclaration> Declarations, List<KarelRoutine> Routines)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    private static Parser<(List<KarelDeclaration>, List<KarelRoutine>)> IncludedFileParser(string programUri)
    {
        if (!File.Exists(programUri))
        {
            return input => Result.Failure<(List<KarelDeclaration>, List<KarelRoutine>)>(input, $"File {programUri} does not exist.", []);
        }

        var includedSource = File.ReadAllText(programUri);
        var parser = from declarations in KarelDeclaration.GetParser().IgnoreComments().Many()
                    from routines in KarelRoutine.GetParser().IgnoreComments().Many()
                    select (declarations.ToList(), routines.ToList());
        return input => parser.TryParse(includedSource) switch
        {
            { WasSuccessful: true } res => Result.Success(res.Value, input),
            { WasSuccessful: false }  res => Result.Failure<(List<KarelDeclaration>, List<KarelRoutine>)>(input, res.Message, res.Expectations)
        };
    }

    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kv in Directive("INCLUDE")
           from fileUri in Parse.CharExcept(['\r', '\n', ';']).Until(KarelCommon.LineBreak).Text()
           from inner in IncludedFileParser(fileUri).WithErrorContext("INCLUDE")
           select new KarelIncludeDirective(Path.GetFileNameWithoutExtension(fileUri), new Uri(fileUri), inner.Item1, inner.Item2);

}

public sealed record KarelLockGroupDirective(List<KarelInteger> LockedGroups)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Directive("LOCKGROUP")
           from sep in KarelCommon.Keyword("=")
           from groups in KarelInteger.GetParser().DelimitedBy(Parse.Char(','))
           select new KarelLockGroupDirective(groups.OfType<KarelInteger>().ToList());
}

public sealed record KarelNoAbortDirective(List<string> Options)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Directive("NOABORT")
           from sep in KarelCommon.Keyword("=")
           from options in KarelCommon.Reserved.DelimitedBy(KarelCommon.Keyword("+"))
           select new KarelNoAbortDirective(options.ToList());
}

public sealed record KarelNoBusyLampDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Directive("NOBUSYLAMP").Return(new KarelNoBusyLampDirective());
}

public sealed record KarelNoLockGroupDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Directive("NOLOCKGROUP").Return(new KarelNoLockGroupDirective());
}

public sealed record KarelNoPauseDirective(List<string> Options)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Directive("NOPAUSE")
           from sep in KarelCommon.Keyword("=")
           from options in KarelCommon.Reserved.DelimitedBy(KarelCommon.Keyword("+"))
           select new KarelNoPauseDirective(options.ToList());
}

public sealed record KarelNoPauseShiftDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Directive("NOPAUSESHFT").Return(new KarelNoPauseShiftDirective());
}

public sealed record KarelPriorityDirective(KarelInteger GroupNumber)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Directive("PRIORITY")
           from sep in KarelCommon.Keyword("=")
           from num in KarelInteger.GetParser()
           select new KarelPriorityDirective((KarelInteger)num);
}

public sealed record KarelShadowVarsDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Directive("SHADOWVARS").Return(new KarelShadowVarsDirective());
}

public sealed record KarelStackSizeDirective(KarelInteger GroupNumber)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Directive("STACKSIZE")
           from sep in KarelCommon.Keyword("=")
           from num in KarelInteger.GetParser()
           select new KarelStackSizeDirective((KarelInteger)num);
}

public sealed record KarelTimeSliceDirective(KarelInteger GroupNumber)
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => from kw in Directive("TIMESLICE")
           from sep in KarelCommon.Keyword("=")
           from num in KarelInteger.GetParser()
           select new KarelTimeSliceDirective((KarelInteger)num);
}

public sealed record KarelTpMotionDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Directive("TPMOTION").Return(new KarelTpMotionDirective());
}

public sealed record KarelUninitVarsDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Directive("UNINITVARS").Return(new KarelUninitVarsDirective());
}
public sealed record KarelSystemDirective
    : KarelTranslatorDirective, IKarelParser<KarelTranslatorDirective>
{
    public new static Parser<KarelTranslatorDirective> GetParser()
        => Directive("SYSTEM").Return(new KarelSystemDirective());
}
