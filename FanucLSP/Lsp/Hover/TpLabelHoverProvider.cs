using FanucLsp.Lsp.State;
using FanucTpLSP.Lsp.Util;
using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;

namespace FanucLsp.Lsp.Hover;

internal sealed class TpLabelHoverProvider : IHoverProvider
{
    public HoverResult? GetHoverResult(TpProgram program, ContentPosition position, LspServerState lspServerState)
    {
        var instruction = program.Main.Instructions.Find(instr => instr.LineNumber - 1 == position.Line);
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
                    Value = $"{(target.LabelNumber as TpAccessDirect)!.Comment} (line {target.Start.Line})"
                },
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
