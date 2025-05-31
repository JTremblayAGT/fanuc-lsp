using ParserUtils;
using Sprache;

namespace KarelParser.Instructions;

public sealed record KarelAbort(KarelAbortTask? Task) : KarelInstruction, IKarelParser<KarelInstruction>
{
    public new static Parser<KarelInstruction> GetParser()
        => from kw in KarelCommon.Keyword("ABORT")
           from task in KarelAbortTask.GetParser().WithPos().Optional()
           select new KarelAbort(task.GetOrElse(null));
}

public sealed record KarelAbortTask(string Identifier, int TaskNumber) : WithPosition, IKarelParser<KarelAbortTask>
{
    public static Parser<KarelAbortTask> GetParser()
        => from ident in KarelCommon.Identifier
           from taskNum in Parse.Number.Select(int.Parse).BetweenBrackets()
           select new KarelAbortTask(ident, taskNum);
}


