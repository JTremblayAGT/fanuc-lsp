using FanucTpLsp.Lsp.State;
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

        var lbl = instruction switch
        {
            TpJumpLabelInstruction jmpLbl => jmpLbl.Label,
            TpMotionInstruction motion => motion.Options.Find(option => option is TpSkipOption or TpSkipJumpOption) switch
            {
                TpSkipOption skip => skip.Label,
                TpSkipJumpOption skipJump => skipJump.Label,
                _ => null
            },
            TpIfInstruction branch => branch.Action switch
            {
                TpJumpLabelInstruction jmpLbl => jmpLbl.Label,
                _ => null,
            },
            TpWaitInstruction wait => wait switch
            {
                TpWaitCondition waitCond => waitCond.TimeoutLabel,
                _ => null,
            },
            TpMixedLogicWaitInstruction wait => wait.TimeoutLabel,
            _ => null
        };

        if (lbl is not { LabelNumber: TpAccessDirect lblNum })
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
            .Where(instr => instr is TpLabelDefinitionInstruction)
            .Select(instr => (instr as TpLabelDefinitionInstruction)!.Label)
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
