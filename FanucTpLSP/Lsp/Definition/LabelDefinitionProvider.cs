using FanucTpLsp.Lsp.State;
using FanucTpLSP.Lsp.Util;
using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;

namespace FanucTpLsp.Lsp.Definition;

internal sealed class TpLabelDefinitionProvider : IDefinitionProvider
{
    public TextDocumentLocation? GetDefinitionLocation(TpProgram program, ContentPosition position, TextDocumentItem document, LspServerState state)
    {
        var instruction = program.Main.Instructions.Find(instr => instr.LineNumber - 1 == position.Line);
        if (instruction == null)
        {
            return null;
        }

        if (TpLabelUtil.GetLabelFromInstruction(instruction) is not { LabelNumber: TpAccessDirect lblNum } lbl)
        {
            return null;
        }

        // Neovim lines are 0-based
        if (position.Line != lbl.Start.Line - 1
            || !(lbl.Start.Column - 1 <= position.Character)
            || !(lbl.End.Column - 1 >= position.Character))
        {
            return null;
        }

        var target = program.Main.Instructions
            .OfType<TpLabelDefinitionInstruction>()
            .Select(instr => instr.Label)
            .FirstOrDefault(lb => lb.LabelNumber is TpAccessDirect direct
                    && direct.Number == lblNum.Number);

        return target switch
        {
            not null => new()
            {
                Uri = document.Uri,
                Range = new()
                {
                    Start = new() { Line = target.Start.Line - 1, Character = target.Start.Column - 1 },
                    End = new() { Line = target.End.Line - 1, Character = target.End.Column - 1 },
                }
            },
            _ => null
        };
    }
}
