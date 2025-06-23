using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelUsing(List<KarelVariableAccess> Variables, List<KarelStatement> Body)
    : KarelStatement, IKarelParser<KarelStatement>
{
    public new static Parser<KarelStatement> GetParser()
        => from kw in KarelCommon.Keyword("USING")
           from vars in KarelVariableAccess.GetParser().DelimitedBy(KarelCommon.Keyword(","), 1, null)
           from kww in KarelCommon.Keyword("DO")
           from body in KarelCommon.ParseStatements(["ENDUSING"])
           from kwww in KarelCommon.Keyword("ENDUSING")
           select new KarelUsing(vars.ToList(), body.ToList());
}
