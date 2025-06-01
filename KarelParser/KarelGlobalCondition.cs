using ParserUtils;
using Sprache;

namespace KarelParser;

public abstract record KarelGlobalCondition
: WithPosition, IKarelParser<KarelGlobalCondition>
{
    public static Parser<KarelGlobalCondition> GetParser()
        => KarelErrorCondition.GetParser()
            .Or(KarelEventCondition.GetParser())
            .Or(KarelSemaphoreCondition.GetParser())
            .Or(KarelAbortCondition.GetParser())
            .Or(KarelPauseCondition.GetParser())
            .Or(KarelContinueCondition.GetParser())
            .Or(KarelPowerUpCondition.GetParser())
            .Or(KarelComparisonCondition.GetParser())
            .Or(KarelPortCondition.GetParser())
            .Or(Parse.Ref(() => GetParser()).BetweenParen())
            .WithPos();
}

public sealed record KarelErrorCondition(KarelExpression Number)
    : KarelGlobalCondition, IKarelParser<KarelGlobalCondition>
{
    public new static Parser<KarelGlobalCondition> GetParser()
        => from kw in KarelCommon.Keyword("ERROR")
           from err in KarelExpression.GetParser().BetweenBrackets() // TODO: add wildcard '*'
           select new KarelErrorCondition(err);
}

public sealed record KarelEventCondition(KarelExpression Number)
    : KarelGlobalCondition, IKarelParser<KarelGlobalCondition>
{
    public new static Parser<KarelGlobalCondition> GetParser()
        => from kw in KarelCommon.Keyword("EVENT")
           from err in KarelExpression.GetParser().BetweenBrackets()
           select new KarelErrorCondition(err);
}

public sealed record KarelSemaphoreCondition(KarelExpression Number)
    : KarelGlobalCondition, IKarelParser<KarelGlobalCondition>
{
    public new static Parser<KarelGlobalCondition> GetParser()
        => from kw in KarelCommon.Keyword("SEMAPHORE")
           from err in KarelExpression.GetParser().BetweenBrackets()
           select new KarelErrorCondition(err);
}

public sealed record KarelAbortCondition(KarelExpression? ProgramNumber)
    : KarelGlobalCondition, IKarelParser<KarelGlobalCondition>
{
    public new static Parser<KarelGlobalCondition> GetParser()
        => from kw in KarelCommon.Keyword("ABORT")
           from taskNum in (from kww in KarelCommon.Keyword("PROGRAM")
                            from taskNum in KarelExpression.GetParser().BetweenBrackets()
                            select taskNum).Optional()
           select new KarelAbortCondition(taskNum.GetOrElse(null));
}

public sealed record KarelPauseCondition(KarelExpression? ProgramNumber)
    : KarelGlobalCondition, IKarelParser<KarelGlobalCondition>
{
    public new static Parser<KarelGlobalCondition> GetParser()
        => from kw in KarelCommon.Keyword("PAUSE")
           from taskNum in (from kww in KarelCommon.Keyword("PROGRAM")
                            from taskNum in KarelExpression.GetParser().BetweenBrackets()
                            select taskNum).Optional()
           select new KarelAbortCondition(taskNum.GetOrElse(null));
}

public sealed record KarelContinueCondition(KarelExpression? ProgramNumber)
    : KarelGlobalCondition, IKarelParser<KarelGlobalCondition>
{
    public new static Parser<KarelGlobalCondition> GetParser()
        => from kw in KarelCommon.Keyword("CONTINUE")
           from taskNum in (from kww in KarelCommon.Keyword("PROGRAM")
                            from taskNum in KarelExpression.GetParser().BetweenBrackets()
                            select taskNum).Optional()
           select new KarelAbortCondition(taskNum.GetOrElse(null));
}

public sealed record KarelPowerUpCondition
    : KarelGlobalCondition, IKarelParser<KarelGlobalCondition>
{
    public new static Parser<KarelGlobalCondition> GetParser()
        => KarelCommon.Keyword("POWERUP").Return(new KarelPowerUpCondition());
}

public record KarelComparisonCondition(
        KarelVariableAcess Variable,
        KarelComparisonOperator Operator,
        KarelExpression Expr) // TODO: make sure this is correct
    : KarelGlobalCondition, IKarelParser<KarelGlobalCondition>
{
    public new static Parser<KarelGlobalCondition> GetParser()
        => from variable in KarelVariableAcess.GetParser()
           from op in KarelComparisonOperatorParser.Parser()
           from expr in KarelExpression.GetParser()
           select new KarelComparisonCondition(variable, op, expr);
}

public record KarelPortCondition(string Identifier, KarelExpression Index, bool Negated, bool Plus)
    : KarelGlobalCondition, IKarelParser<KarelGlobalCondition>
{
    public new static Parser<KarelGlobalCondition> GetParser()
        => from negated in KarelCommon.Keyword("NOT").Optional()
           from ident in KarelCommon.Identifier
           from idx in KarelExpression.GetParser().BetweenBrackets()
           from plus in KarelCommon.Keyword("+").Optional()
           select new KarelPortCondition(ident, idx, negated.IsDefined, plus.IsDefined);
}

