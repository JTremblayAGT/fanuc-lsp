using FanucLsp.Lsp.State;
using Sprache;
using TPLangParser.TPLang;

namespace FanucLsp.Lsp.Completion;

internal sealed class TpCallCompletionProvider : ICompletionProvider
{

    public CompletionItem[] GetCompletions(TpProgram program, string lineText, int column, LspServerState serverState)
        => CompletionProviderUtils.TokenizeInput(lineText[..column]) switch
        {
            [.., "CALL"] => CompletionProviderUtils.GetAllProgramNames(serverState),
            [.., "RUN"] => CompletionProviderUtils.GetAllProgramNames(serverState),
            _ => []
        };

}
