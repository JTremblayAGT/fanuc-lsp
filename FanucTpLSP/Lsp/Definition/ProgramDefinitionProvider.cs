using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;
using FanucTpLsp.Lsp.State;

namespace FanucTpLsp.Lsp.Definition;

internal class TpProgramDefinitionProvider : IDefinitionProvider
{
    public TextDocumentLocation? GetDefinitionLocation(
            TpProgram program,
            ContentPosition position,
            TextDocumentItem document,
            LspServerState state)
    {
        // TODO: ensure the token at [position] is a program name (in a CALL instruction)
        // Search all documents for a file with that name and open it at line 1

        var instr = program.Main.Instructions.Find(instr => instr.LineNumber - 1 == position.Line);
        if (instr == null)
        {
            return null;
        }

        var call = instr switch
        {
            TpCallInstruction callInstr => callInstr.CallMethod switch
            {
                TpCallByName => callInstr,
                _ => null
            },
            TpRunInstruction runInstr => new TpCallInstruction(runInstr.ProgramName, []), // cheeky hack LOLE
            TpIfInstruction branch => branch.Action switch
            {
                TpCallInstruction callAction => callAction,
                _ => null
            },
            _ => null
        };

        if (call?.CallMethod is not TpCallByName callByName)
        {
            return null;
        }

        if (callByName.Start.Column - 1 > position.Character
            || callByName.End.Column - 1 < position.Character)
        {
            return null;
        }

        // TODO: find program in all documents
        var target = state.AllTextDocuments
            .FirstOrDefault(kvp => Path.GetFileNameWithoutExtension(kvp.Key)
                .Equals(callByName.ProgramName, StringComparison.OrdinalIgnoreCase));

        return target.Value switch
        {
            { } doc => new()
            {
                Uri = doc.TextDocument.Uri,
                Range = new()
                {
                    Start = new() { Line = 0, Character = 0 },
                    End = new() { Line = 0, Character = 0 },
                }
            },
            _ => null
        };
    }
}
