using FanucLsp.Lsp.State;
using FanucLsp.Lsp.Util;
using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;

namespace FanucLsp.Lsp.Hover;

internal sealed class TpLabelHoverProvider : IHoverProvider
{
    public HoverResult? GetHoverResult(TpProgram program, ContentPosition position, LspServerState lspServerState)
    {
        var instruction = program.Main.Instructions.Find(instr => instr.Start.Line == position.Line);
        if (instruction == null)
        {
            return null;
        }

        if (TpLabelUtil.GetLabelFromInstruction(instruction) is not { LabelNumber: TpAccessDirect lblNum })
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
                Contents = new()
                {
                    Kind = "plaintext",
                    Value = $"{(target.LabelNumber as TpAccessDirect)!.Comment} (line {target.Start.Line + 1})"
                },
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
