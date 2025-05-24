using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;

namespace FanucTpLsp.Lsp.Completion;

public class TpLabelCompletion : ICompletionProvider
{
    public CompletionItem[] GetCompletions(TpProgram program, string lineText, int column)
    {
        var prefix = lineText[..column];

        if (!prefix.Contains(TpLabel.Keyword))
        {
            return [];
        }

        var tokens = CompletionProviderUtils.TokenizeInput(prefix);

        if (tokens is { Count: 1 } && tokens.First().StartsWith(TpLabel.Keyword))
        {
            // Do not return anything for label declaration instructions
            return [];
        }

        if (tokens.Count > 1 && !tokens.Last().Contains(TpLabel.Keyword))
        {
            return [];
        }

        return GetLabelDescriptions(program).Concat([new()
        {
            Label = $"{TpRegister.Keyword}[n]",
            Detail = "Indirect label access",
            Documentation = "Jump to the label number stored in the register",
            InsertText = $"{TpRegister.Keyword}[$1]",
            InsertTextFormat = InsertTextFormat.Snippet,
            Kind = CompletionItemKind.Snippet,
        }]).ToArray();
    }

    private static CompletionItem[] GetLabelDescriptions(TpProgram program)
        => program.Main.Instructions.OfType<TpLabelDefinitionInstruction>()
            .Select(labelDef => labelDef.Label.LabelNumber as TpAccessDirect)
            .Select(access => new CompletionItem
            {
                Label = $"{access!.Number} : {(!string.IsNullOrWhiteSpace(access.Comment) ? access.Comment : "(no comment)")}",
                Detail = string.Empty,
                Documentation = string.Empty,
                InsertText = $"{access.Number}",
                InsertTextFormat = InsertTextFormat.PlainText,
                Kind = CompletionItemKind.Value,
                SortText = $"{access.Number}",
            }).ToArray();
}
