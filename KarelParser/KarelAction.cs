using ParserUtils;
using Sprache;
using KarelParser.Instructions;

namespace KarelParser;

public abstract record KarelAction : WithPosition, IKarelParser<KarelAction>
{
    public static Parser<KarelAction> GetParser()
        => KarelGroupAction.GetParser()
            .Or(KarelConditionAction.GetParser())
            .Or(KarelEventAction.GetParser())
            .Or(KarelSemaphoreAction.GetParser())
            .Or(KarelPulseDoutAction.GetParser())
            .Or(KarelPulseRdoAction.GetParser())
            .Or(KarelNoAbortAction.GetParser())
            .Or(KarelNoPauseAction.GetParser())
            .Or(KarelUnpauseAction.GetParser())
            .Or(KarelNoMessageAction.GetParser())
            .Or(KarelRestoreAction.GetParser())
            .Or(KarelAbortAction.GetParser())
            .Or(KarelContinueAction.GetParser())
            .Or(KarelPauseAction.GetParser())
            .Or(KarelAssignmentAction.GetParser())
            .WithPos();
}

public sealed record KarelGroupAction(KarelStatement Statement)
    : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => (
            KarelHold.GetParser()
            .Or(KarelUnhold.GetParser())
            .Or(KarelResume.GetParser())
            .Or(KarelStop.GetParser())
            .Or(KarelCancel.GetParser())
        ).Select(stmt => new KarelGroupAction(stmt));
}

public sealed record KarelConditionAction(KarelStatement Statement)
    : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => (
            KarelEnable.GetParser()
            .Or(KarelDisable.GetParser())
        ).Select(stmt => new KarelConditionAction(stmt));
}

public sealed record KarelEventAction(KarelSignal Signal)
    : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => KarelSignal.GetParser().Select(stmt => new KarelEventAction((KarelSignal)stmt));
}

public sealed record KarelSemaphoreAction(KarelExpression Number)
    : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => from kw in KarelCommon.Keyword("SIGNAL")
           from kww in KarelCommon.Keyword("SEMAPHORE")
           from num in KarelExpression.GetParser().BetweenBrackets()
           select new KarelSemaphoreAction(num);
}

public sealed record KarelPulseDoutAction(KarelExpression Index, KarelExpression Time) : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => from kw in KarelCommon.Keyword("PULSE")
           from kww in KarelCommon.Keyword("DOUT")
           from index in KarelExpression.GetParser().BetweenBrackets()
           from kwww in KarelCommon.Keyword("FOR")
           from time in KarelExpression.GetParser()
           select new KarelPulseDoutAction(index, time);
}

public sealed record KarelPulseRdoAction(KarelExpression Index, KarelExpression Time) : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => from kw in KarelCommon.Keyword("PULSE")
           from kww in KarelCommon.Keyword("RDO")
           from index in KarelExpression.GetParser().BetweenBrackets()
           from kwww in KarelCommon.Keyword("FOR")
           from time in KarelExpression.GetParser()
           select new KarelPulseRdoAction(index, time);
}

public sealed record KarelNoAbortAction : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => KarelCommon.Keyword("NOABORT").Return(new KarelNoAbortAction());
}

public sealed record KarelNoPauseAction : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => KarelCommon.Keyword("NOPAUSE").Return(new KarelNoPauseAction());
}

public sealed record KarelUnpauseAction : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => KarelCommon.Keyword("UNPAUSE").Return(new KarelUnpauseAction());
}

public sealed record KarelNoMessageAction : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => KarelCommon.Keyword("NOMESSAGE").Return(new KarelNoMessageAction());
}

public sealed record KarelRestoreAction : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => KarelCommon.Keyword("RESTORE").Return(new KarelRestoreAction());
}

public sealed record KarelAbortAction(KarelExpression? ProgramNumber)
    : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => from kw in KarelCommon.Keyword("ABORT")
           from taskNum in (from kww in KarelCommon.Keyword("PROGRAM")
                            from taskNum in KarelExpression.GetParser().BetweenBrackets()
                            select taskNum).Optional()
           select new KarelAbortAction(taskNum.GetOrElse(null));
}

public sealed record KarelPauseAction(KarelExpression? ProgramNumber)
    : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => from kw in KarelCommon.Keyword("PAUSE")
           from taskNum in (from kww in KarelCommon.Keyword("PROGRAM")
                            from taskNum in KarelExpression.GetParser().BetweenBrackets()
                            select taskNum).Optional()
           select new KarelAbortAction(taskNum.GetOrElse(null));
}

public sealed record KarelContinueAction(KarelExpression? ProgramNumber)
    : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => from kw in KarelCommon.Keyword("CONTINUE")
           from taskNum in (from kww in KarelCommon.Keyword("PROGRAM")
                            from taskNum in KarelExpression.GetParser().BetweenBrackets()
                            select taskNum).Optional()
           select new KarelAbortAction(taskNum.GetOrElse(null));
}

public abstract record KarelAssignmentAction : KarelAction, IKarelParser<KarelAction>
{
    public new static Parser<KarelAction> GetParser()
        => KarelPortAssignmentAction.GetParser()
            .Or(KarelVarAssignmentAction.GetParser());
}

public sealed record KarelPortAssignmentAction(string Identifier, KarelExpression Index, KarelExpression Expr)
    : KarelAssignmentAction, IKarelParser<KarelAssignmentAction>
{
    static Parser<KarelAssignmentAction> IKarelParser<KarelAssignmentAction>.GetParser()
        => from ident in KarelCommon.Identifier
           from index in KarelExpression.GetParser().BetweenBrackets()
           from sep in KarelCommon.Keyword("=")
           from expr in KarelExpression.GetParser()
           select new KarelPortAssignmentAction(ident, index, expr);
}

public sealed record KarelVarAssignmentAction(KarelVariableAccess Variable, KarelExpression Expr)
    : KarelAssignmentAction, IKarelParser<KarelAssignmentAction>
{
    static Parser<KarelAssignmentAction> IKarelParser<KarelAssignmentAction>.GetParser()
        => from ident in KarelVariableAccess.GetParser()
           from sep in KarelCommon.Keyword("=")
           from expr in KarelExpression.GetParser()
           select new KarelVarAssignmentAction(ident, expr);
}

