using ParserUtils;
using Sprache;
using KarelParser.Instructions;

namespace KarelParser;

public abstract record KarelStatement() : WithPosition, IKarelParser<KarelStatement>
{
    public static Parser<KarelStatement> GetParser()
        => KarelAbort.GetParser()
            .Or(KarelAssignment.GetParser())
            .Or(KarelAttach.GetParser())
            .Or(KarelCall.GetParser())
            .Or(KarelCancel.GetParser())
            .Or(KarelCancelFile.GetParser())
            .Or(KarelCloseFile.GetParser())
            .Or(KarelCloseHand.GetParser())
            .Or(KarelCondition.GetParser())
            .Or(KarelConnectTimer.GetParser())
            .Or(KarelDelay.GetParser())
            .Or(KarelDisable.GetParser())
            .Or(KarelDisconnectTimer.GetParser())
            .Or(KarelEnable.GetParser())
            .Or(KarelFor.GetParser())
            .Or(KarelGoto.GetParser())
            .Or(KarelHold.GetParser())
            .Or(KarelIfThen.GetParser())
            .Or(KarelOpenFile.GetParser())
            .Or(KarelOpenHand.GetParser())
            .Or(KarelPause.GetParser())
            .Or(KarelPulse.GetParser())
            .Or(KarelPurge.GetParser())
            .Or(KarelRead.GetParser())
            .Or(KarelRelaxHand.GetParser())
            .Or(KarelRelease.GetParser())
            .Or(KarelRepeat.GetParser())
            .Or(KarelResume.GetParser())
            .Or(KarelReturn.GetParser())
            .Or(KarelSelect.GetParser())
            .Or(KarelSignal.GetParser())
            .Or(KarelStop.GetParser())
            .Or(KarelUnhold.GetParser())
            .Or(KarelUsing.GetParser())
            .Or(KarelWait.GetParser())
            .Or(KarelWhile.GetParser())
            .Or(KarelWrite.GetParser());
}

public abstract record KarelExpression() : WithPosition, IKarelParser<KarelExpression>
{
    public static Parser<KarelExpression> GetParser()
        // TODO:
        => throw new NotImplementedException();
}

