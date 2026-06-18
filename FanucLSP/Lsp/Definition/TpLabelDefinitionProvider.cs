using FanucLsp.Lsp.State;
using FanucLsp.Lsp.Util;
using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;

namespace FanucLsp.Lsp.Definition;

internal sealed class TpLabelDefinitionProvider : ITpDefinitionProvider
{
    public TextDocumentLocation? GetDefinitionLocation(TpProgram program, ContentPosition position, TextDocumentItem document, LspServerState state)
    {
        var instruction = program.Main.Instructions.Find(instr => instr.Start.Line == position.Line);
        if (instruction == null)
        {
            return null;
        }

        if (TpLabelUtil.GetLabelFromInstruction(instruction) is not { LabelNumber: TpAccessDirect lblNum } lbl)
        {
            return null;
        }

        // Neovim lines are 0-based
        if (position.Line != lbl.Start.Line
            || !(lbl.Start.Column <= position.Character)
            || !(lbl.End.Column >= position.Character))
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
                    Start = new() { Line = target.Start.Line, Character = target.Start.Column },
                    End = new() { Line = target.End.Line, Character = target.End.Column },
                }
            },
            _ => null
        };
    }
}
