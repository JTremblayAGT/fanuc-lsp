using FanucTpLsp.Lsp.State;
using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;

namespace FanucTpLsp.Lsp.Hover;

internal sealed class CallHoverProvider : IHoverProvider
{
    public HoverResult? GetHoverResult(TpProgram program, ContentPosition position, LspServerState serverState)
        => program.Main.Instructions.FirstOrDefault(instr => instr.LineNumber == position.Line + 1) switch
        {
            TpCallInstruction callInstruction => callInstruction.CallMethod switch
            {
                TpCallByName byName => GetHoverFromCall(byName, position, serverState),
                _ => null
            },
            TpIfInstruction ifInstr => ifInstr.Action switch
            {
                TpCallInstruction callInstruction => callInstruction.CallMethod switch
                {
                    TpCallByName byName => GetHoverFromCall(byName, position, serverState),
                    _ => null
                },
                _ => null
            },
            _ => null,
        };

    private HoverResult? GetHoverFromCall(TpCallByName byName, ContentPosition position, LspServerState serverState)
        => position.Character switch
        {
            { } ch when ch >= byName.Start.Column && ch <= byName.End.Column
                => MakeHoverResult(byName.ProgramName, byName.Start, byName.End, serverState),
            _ => null
        };


    private HoverResult? MakeHoverResult(
            string programName,
            TpTokenPosition start,
            TpTokenPosition end,
            LspServerState serverState)
    {
        if (serverState.AllTextDocuments.FirstOrDefault(
            kvp => Path.GetFileNameWithoutExtension(kvp.Key)
                .Equals(programName, StringComparison.OrdinalIgnoreCase)) is not { Key: not null and not "" } found)
        {
            return null;
        }
        var program = found.Value;

        return new HoverResult()
        {
            Contents = new()
            {
                Kind = "markdown",
                Value = $"**{programName.ToUpper()}** ({program.Type})\n"
                      + $"*{program.TextDocument.Uri}*\n\n"
                      + $"{LspUtils.ExtractDocComment(program.Program)}"
            },
            Range = new()
            {
                Start = new() { Line = start.Line - 1, Character = start.Column - 1 },
                End = new() { Line = end.Line - 1, Character = end.Column - 1 },
            }
        };
    }
}

