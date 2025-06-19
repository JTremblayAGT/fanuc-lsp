using ParserUtils;
using Sprache;

namespace TPLangParser.TPLang.Instructions;

public record TpMixedLogicInstruction() : TpInstruction(0), ITpParser<TpMixedLogicInstruction>
{
    public new static Parser<TpMixedLogicInstruction> GetParser() 
        => TpMixedLogicAssignment.GetParser()
            .Or(TpMixedLogicWaitInstruction.GetParser());
}

public sealed record TpMixedLogicAssignment(TpValue Assignable, TpMixedLogicExpression Expression)
    : TpMixedLogicInstruction, ITpParser<TpMixedLogicInstruction>
{
    public new static Parser<TpMixedLogicInstruction> GetParser() 
        => from assignable in TpValue.Assignable.Token()
            from sep in TpCommon.Keyword("=")
            from expr in TpMixedLogicExpression.GetParser()
            select new TpMixedLogicAssignment(assignable, expr);
}

public sealed record TpMixedLogicWaitInstruction(TpMixedLogicExpression Expression, TpLabel? TimeoutLabel)
    : TpMixedLogicInstruction, ITpParser<TpMixedLogicInstruction>
{
    public new static Parser<TpMixedLogicInstruction> GetParser() 
        => from keyword in TpCommon.Keyword("WAIT")
            from expr in TpMixedLogicExpression.GetParser().BetweenParen()
            from label in
                (from keyword2 in TpCommon.Keyword("TIMEOUT")
                    from sep2 in TpCommon.Keyword(",")
                    from label in TpLabel.GetParser()
                    select label).Optional()
            select new TpMixedLogicWaitInstruction(expr, label.GetOrDefault());
}