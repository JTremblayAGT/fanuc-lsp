using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;

namespace FanucTpLsp.Lsp.Definition;

internal sealed class TpLabelDefinitionProvider : IDefinitionProvider
{
    public TextDocumentLocation? GetDefinitionLocation(TpProgram program, ContentRange range, TextDocumentItem document)
    {
        var instruction = program.Main.Instructions.Find(instr => instr.LineNumber == range.Start.Line);
        if (instruction == null)
        {
            return null;
        }

        TpLabel? lbl = instruction switch
        {
            TpJumpLabelInstruction jmpLbl => jmpLbl.Label,
            TpMotionInstruction motion =>
                motion.Options.Find(option => option is TpSkipOption || option is TpSkipJumpOption) switch
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

        if (lbl == null)
        {
            return null;
        }

        // TODO: TpPositionedToken needs a RANGE (start + end) instead of just a single position

        // TODO: Find the TpLabelDeclarationInstruction with the same number and return its position
        return null;
    }
}
