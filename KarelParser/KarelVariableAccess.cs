using ParserUtils;

using Sprache;

namespace KarelParser;

public abstract record KarelVariableAcess : KarelValue, IKarelParser<KarelVariableAcess>
{
    public new static Parser<KarelVariableAcess> GetParser()
        => KarelFieldAccess.GetParser()
            .Or(KarelIdentifier.GetParser())
            .WithPos();
}

public sealed record KarelIdentifier(string Identifier) : KarelVariableAcess, IKarelParser<KarelVariableAcess>
{
    public new static Parser<KarelVariableAcess> GetParser()
        => KarelCommon.Identifier.Select(ident => new KarelIdentifier(ident));
}

public sealed record KarelFieldAccess(KarelVariableAcess Variable, string Field)
    : KarelVariableAcess, IKarelParser<KarelVariableAcess>
{
    public new static Parser<KarelVariableAcess> GetParser()
        => from variable in Parse.Ref(() => KarelVariableAcess.GetParser())
           from sep in Parse.Char('.')
           from field in KarelCommon.Identifier
           select new KarelFieldAccess(variable, field);
}

public sealed record KarelArrayAccess(KarelVariableAcess Variable, List<KarelExpression> Indices)
    : KarelVariableAcess, IKarelParser<KarelVariableAcess>
{
    public new static Parser<KarelVariableAcess> GetParser()
        => from variable in Parse.Ref(() => KarelVariableAcess.GetParser())
           from indices in KarelExpression.GetParser()
           .DelimitedBy(KarelCommon.Keyword(","), 1, null)
           .BetweenBrackets()
           select new KarelArrayAccess(variable, indices.ToList());
}

public sealed record KarelPathAccess(KarelVariableAcess Variable, KarelExpression StartNode, KarelExpression EndNode)
    : KarelVariableAcess, IKarelParser<KarelVariableAcess>
{
    public new static Parser<KarelVariableAcess> GetParser()
        => from variable in Parse.Ref(() => KarelVariableAcess.GetParser())
           from lbracket in KarelCommon.Keyword("[")
           from startnode in KarelExpression.GetParser()
           from sep in KarelCommon.Keyword("..")
           from endnode in KarelExpression.GetParser()
           from rbracket in KarelCommon.Keyword("[")
           select new KarelPathAccess(variable, startnode, endnode);
}

