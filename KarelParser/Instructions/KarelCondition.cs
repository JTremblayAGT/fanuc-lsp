using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelCondition(KarelExpression HandlerNumber, KarelWith? With, List<KarelWhen> When)
    : KarelInstruction, IKarelParser<KarelInstruction>
{
    public new static Parser<KarelInstruction> GetParser()
        => from kw in KarelCommon.Keyword("CONDITION")
           from handler in KarelExpression.GetParser().BetweenBrackets()
           from sep in KarelCommon.Keyword(":")
           from with in KarelWith.GetParser().Optional()
           from cond in KarelWhen.GetParser().DelimitedBy(KarelCommon.LineBreak, 1, null)
           from brk in KarelCommon.LineBreak
           from kww in KarelCommon.Keyword("ENDCONDITION")
           select new KarelCondition(handler, with.GetOrElse(null), cond.ToList());
}

public sealed record KarelWith(List<KarelWithAssignment> Assignments) : WithPosition, IKarelParser<KarelWith>
{
    public static Parser<KarelWith> GetParser()
        => from kw in KarelCommon.Keyword("WITH")
           from assignments in KarelWithAssignment.GetParser()
                .WithPos()
                .DelimitedBy(Parse.Char(',').Then(_ => KarelCommon.LineBreak.Optional()))
           select new KarelWith(assignments.ToList());
}

public sealed record KarelWithAssignment(KarelSystemIndentifier Indentifier, KarelExpression Expr)
    : WithPosition, IKarelParser<KarelWithAssignment>
{
    public static Parser<KarelWithAssignment> GetParser()
        => from sysIdent in KarelSystemIndentifier.GetParser().WithPos()
           from sep in KarelCommon.Keyword("=")
           from expr in KarelExpression.GetParser()
           select new KarelWithAssignment((KarelSystemIndentifier)sysIdent, expr);
}

public record KarelWhen(KarelWhenCondition Condition, List<KarelAction> Actions) : WithPosition, IKarelParser<KarelWhen>
{
    public static Parser<KarelWhen> GetParser()
        => from kw in KarelCommon.Keyword("WHEN")
           from cond in KarelWhenCondition.GetParser().WithPos()
           from kww in KarelCommon.Keyword("DO")
           from actions in KarelAction.GetParser()
                .DelimitedBy(KarelCommon.Keyword(",").Or(KarelCommon.LineBreak), 1, null)
           select new KarelWhen(cond, actions.ToList());
}
public record KarelWhenCondition : WithPosition, IKarelParser<KarelWhenCondition>
{
    public static Parser<KarelWhenCondition> GetParser()
        => KarelWhenOr.GetParser()
            .Or(KarelWhenAnd.GetParser());
}

public sealed record KarelWhenOr(List<KarelGlobalCondition> Conditions) : KarelWhenCondition, IKarelParser<KarelWhenCondition>
{
    public new static Parser<KarelWhenCondition> GetParser()
        => from conds in KarelGlobalCondition.GetParser().DelimitedBy(KarelCommon.Keyword("OR"))
           select new KarelWhenOr(conds.ToList());
}

public sealed record KarelWhenAnd(List<KarelGlobalCondition> Conditions) : KarelWhenCondition, IKarelParser<KarelWhenCondition>
{
    public new static Parser<KarelWhenCondition> GetParser()
        => from conds in KarelGlobalCondition.GetParser().DelimitedBy(KarelCommon.Keyword("AND"))
           select new KarelWhenAnd(conds.ToList());
}


