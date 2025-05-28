using FanucTpLsp.Lsp.State;
using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;

namespace FanucTpLsp.Lsp.Completion;

public class TpLabelCompletionProvider : ICompletionProvider
{
    public CompletionItem[] GetCompletions(TpProgram program, string lineText, int column, LspServerState serverState)
    {
        var tokens = CompletionProviderUtils.TokenizeInput(lineText[..column]);

        switch (tokens)
        {
            case { Count: 0 }:
                // Suggest label declaration if line is empty
                return [
                    new()
                    {
                        Label = $"{TpLabel.Keyword}[n:<comment>]",
                        Detail = "Label declaration instruction",
                        Documentation = "Declares a label that can be jumped to.",
                        InsertText = $"{TpLabel.Keyword}[$1:$2]",
                        InsertTextFormat = InsertTextFormat.Snippet,
                        Kind = CompletionItemKind.Snippet
                    }
                ];
            case { Count: 1 } when tokens.First().StartsWith(TpLabel.Keyword):
                // Do not return anything for label declaration instructions
                return [];
        }

        if (tokens.Count > 1 && !tokens.Last().Contains(TpLabel.Keyword))
        {
            return [];
        }

        return GetLabelCompletions(program).Concat(IndirectLabelCompletion).ToArray();
    }

    private static CompletionItem[] IndirectLabelCompletion
        => [
            new()
            {
                Label = $"{TpRegister.Keyword}[n]",
                Detail = "Indirect label access",
                Documentation = "Jump to the label number stored in the register",
                InsertText = $"{TpRegister.Keyword}[$1]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            }
        ];

    private static CompletionItem[] GetLabelCompletions(TpProgram program)
        => program.Main.Instructions.OfType<TpLabelDefinitionInstruction>()
            .Select(labelDef => labelDef.Label.LabelNumber as TpAccessDirect)
            .Select(access => new CompletionItem
            {
                Label = $"{access!.Number} : {(!string.IsNullOrWhiteSpace(access!.Comment) ? access!.Comment : "(no comment)")}",
                Detail = string.Empty,
                Documentation = string.Empty,
                InsertText = $"{access!.Number}",
                InsertTextFormat = InsertTextFormat.PlainText,
                Kind = CompletionItemKind.Value
            }).ToArray();
}
