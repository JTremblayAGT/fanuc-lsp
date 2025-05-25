using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;

namespace FanucTpLsp.Lsp.Hover;

internal sealed class TpLabelHoverProvider : IHoverProvider
{
    public HoverResult? GetHoverResult(TpProgram program, ContentPosition position)
    {
        var instruction = program.Main.Instructions.Find(instr => instr.LineNumber - 1 == position.Line);
        if (instruction == null)
        {
            return null;
        }

        var lbl = instruction switch
        {
            TpJumpLabelInstruction jmpLbl => jmpLbl.Label,
            TpMotionInstruction motion =>
                motion.Options.Find(option => option is TpSkipOption or TpSkipJumpOption) switch
                {
                    TpSkipOption skip => skip.Label,
                    TpSkipJumpOption skipJump => skipJump.Label,
                    _ => null
                },
            TpIfInstruction branch =>
                branch.Action switch
                {
                    TpJumpLabelInstruction jmpLbl => jmpLbl.Label,
                    _ => null,
                },
            _ => null
        };

        if (lbl is not { LabelNumber: TpAccessDirect lblNum })
        {
            return null;
        }

        var target = program.Main.Instructions
            .Where(instr => instr is TpLabelDefinitionInstruction)
            .Select(instr => (instr as TpLabelDefinitionInstruction)!.Label)
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
