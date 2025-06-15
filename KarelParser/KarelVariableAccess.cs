using Sprache;

namespace KarelParser;

public abstract record KarelVariableAccess : KarelValue, IKarelParser<KarelVariableAccess>
{
    public new static Parser<KarelVariableAccess> GetParser()
        => KarelIdentifier.GetParser()
            .Then(baseVar =>
                Parse.Ref(() => FieldSuffix.Or(ArraySuffix).Or(PathSuffix))
                .Many()
                .Select(suffixes =>
                    suffixes.Aggregate(baseVar, (acc, suffix) => suffix(acc))
                )
            ) ;

    private static readonly Parser<Func<KarelVariableAccess, KarelVariableAccess>> FieldSuffix =
        from dot in Parse.Char('.')
        from field in KarelCommon.Identifier
        select new Func<KarelVariableAccess, KarelVariableAccess>(
            baseVar => new KarelFieldAccess(baseVar, field));

    private static readonly Parser<Func<KarelVariableAccess, KarelVariableAccess>> ArraySuffix =
        from indices in ExprRef
            .DelimitedBy(KarelCommon.Keyword(","), 1, null)
            .Contained(Parse.Char('['), Parse.Char(']'))
        select new Func<KarelVariableAccess, KarelVariableAccess>(
            baseVar => new KarelArrayAccess(baseVar, indices.ToList()));

    private static readonly Parser<Func<KarelVariableAccess, KarelVariableAccess>> PathSuffix =
        from lbracket in Parse.Char('[')
        from startNode in ExprRef
        from range in Parse.String("..")
        from endNode in ExprRef
        from rbracket in Parse.Char(']')
        select new Func<KarelVariableAccess, KarelVariableAccess>(
            baseVar => new KarelPathAccess(baseVar, startNode, endNode));
}

public sealed record KarelIdentifier(string Identifier) : KarelVariableAccess
{
    public new static Parser<KarelVariableAccess> GetParser()
        => KarelCommon.Identifier.Select(ident =>
            new KarelIdentifier(ident));
}

public sealed record KarelFieldAccess(KarelVariableAccess Variable, string Field)
    : KarelVariableAccess;

public sealed record KarelArrayAccess(KarelVariableAccess Variable, List<KarelExpression> Indices)
    : KarelVariableAccess;

public sealed record KarelPathAccess(KarelVariableAccess Variable, KarelExpression StartNode, KarelExpression EndNode)
    : KarelVariableAccess;

