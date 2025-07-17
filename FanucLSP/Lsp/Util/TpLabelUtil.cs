using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;

namespace FanucTpLSP.Lsp.Util;

public class TpLabelUtil
{
    public static TpLabel? GetLabelFromInstruction(TpInstruction instruction)
        => instruction switch
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
            TpSelectInstruction sel => GetLabelFromAction(sel.Case.Action),
            TpSelectCaseInstruction sel => GetLabelFromAction(sel.Case.Action),
            _ => null
        };

    private static TpLabel? GetLabelFromAction(TpBranchingAction action)
        => action switch
        {
            TpJumpLabelInstruction jmpLbl => jmpLbl.Label,
            _ => null,
        };

}