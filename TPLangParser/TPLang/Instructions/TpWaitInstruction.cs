using Sprache;

namespace TPLangParser.TPLang.Instructions;

public record TpWaitInstruction() : TpInstruction(0), ITpParser<TpWaitInstruction>
{
    // Mixed logic wait is implemented in the mixed logic instructions
    public new static Parser<TpWaitInstruction> GetParser() 
        => TpWaitCondition.GetParser()
            .Or(TpWaitTime.GetParser());
}

public sealed record TpWaitTime(TpValue WaitTime) : TpWaitInstruction, ITpParser<TpWaitInstruction>
{
    public new static Parser<TpWaitInstruction> GetParser()
        => from keyword in TpCommon.Keyword("WAIT")
            from waitTime in TpValue.GetParser()
            from tail in TpCommon.Keyword("(sec)").Optional()
            select new TpWaitTime(waitTime);
}

public sealed record TpWaitCondition(TpLogicExpression Condition, TpLabel? TimeoutLabel)
    : TpWaitInstruction, ITpParser<TpWaitInstruction>
{
    public new static Parser<TpWaitInstruction> GetParser()
        => from keyword in TpCommon.Keyword("WAIT")
            from condition in TpLogicExpression.GetParser()
            from label in
                (from keyword2 in TpCommon.Keyword("TIMEOUT")
                    from sep2 in TpCommon.Keyword(",")
                    from label in TpLabel.GetParser()
                    select label).Optional()
            select new TpWaitCondition(condition, label.GetOrDefault());
}
