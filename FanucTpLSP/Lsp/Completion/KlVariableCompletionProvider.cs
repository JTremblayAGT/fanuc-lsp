using FanucTpLsp.Lsp.State;
using KarelParser;
using TPLangParser.TPLang;

using System.Text.RegularExpressions;

namespace FanucTpLsp.Lsp.Completion;

internal sealed partial class KlVariableCompletionProvider : ICompletionProvider
{

    [GeneratedRegex(@"\$\[[^\]]*")]
    private static partial Regex ProgramName();

    [GeneratedRegex(@"\$\[([a-zA-Z_]+)\]([a-zA-Z_]*(\[[1-9]+\])?\.)*")]
    private static partial Regex Variable();

    public CompletionItem[] GetCompletions(TpProgram program, string lineText, int column, LspServerState serverState)
        => CompletionProviderUtils.TokenizeInput(lineText[..column]) switch
        {
            [.., string variable] when variable.StartsWith('$') => CompleteVariable(variable, serverState),
            _ => []
        };

    private CompletionItem[] CompleteVariable(string variable, LspServerState serverState)
        => variable switch
        {
            string partialVar when Variable().IsMatch(partialVar) => GetVariableCompletions(partialVar, serverState),
            string prog when ProgramName().IsMatch(prog) => CompletionProviderUtils.GetKarelProgramNames(serverState),
            _ => []
        };

    private static CompletionItem[] GetVariableCompletions(string partialVar, LspServerState serverState)
    {
        var match = Variable().Match(partialVar);
        var programName = match.Groups[0].Value;

        if (serverState.AllTextDocuments
                .FirstOrDefault(kvp => Path.GetFileNameWithoutExtension(kvp.Key)
                    .Equals(programName, StringComparison.OrdinalIgnoreCase))
                .Value.Program is not KlProgram klProg)
        {
            return [];
        }

        var labels = partialVar.Replace($"$[{programName.ToUpper()}]", string.Empty).Split('.');
        // Add Base Vars

        return labels.Length switch
        {
            0 or 1 => klProg.Program.Declarations
            .OfType<KarelVariableDeclaration>()
            .SelectMany(decl => decl.Variable.Select(kvar => new CompletionItem
            {
                Label = kvar.Identifier,
                Detail = kvar.Type.ToString(),
                Documentation = string.Empty,
                InsertText = kvar.Identifier.ToUpper(),
                InsertTextFormat = InsertTextFormat.PlainText,
                Kind = CompletionItemKind.Variable
            })).ToArray(),
            _ => TraverseVariables(labels, klProg.Program)
        };
    }

    private static CompletionItem[] TraverseVariables(string[] labels, KarelProgram prog)
    {
        if (labels.Length < 2)
        {
            return [];
        }
        var currLabel = labels[0].Remove(labels[0].IndexOf('['));
        if (prog.Declarations
                .OfType<KarelVariableDeclaration>()
                .SelectMany(decl => decl.Variable)
                .FirstOrDefault(kvar => kvar.Identifier.Equals(currLabel, StringComparison.OrdinalIgnoreCase))
                is not { } karelVar)
        {
            return [];
        }

        var structures = prog.Declarations
            .OfType<KarelTypeDeclaration>()
            .SelectMany(decl => decl.Type)
            .Where(typ =>
            {
                if (typ.Type is not KarelUserType userType)
                {
                    return false;
                }

                return userType is KarelStructure;
            })
            .ToDictionary(typ => typ.Identifier, typ => (KarelStructure)((KarelUserType)typ.Type));

        return karelVar.Type as KarelDataType switch
        {
            KarelTypeName typeName => TraverseIfStructure(labels[1..], typeName, structures),
            KarelTypeArray arrayType => arrayType.Type switch
            {
                KarelTypeName typeName => TraverseIfStructure(labels[1..], typeName, structures),
                _ => []
            },
            _ => []
        };
    }

    private static CompletionItem[] TraverseIfStructure(string[] labels, KarelTypeName typeName, Dictionary<string, KarelStructure> structures)
    {
        var currLabel = labels[0].Remove(labels[0].IndexOf('['));
        if (structures.TryGetValue(currLabel, out var structure))
        {
            return [];
        }

        return TraverseFields(labels, structure!, structures);
    }

    private static CompletionItem[] TraverseFields(string[] labels, KarelStructure structure, Dictionary<string, KarelStructure> structures)
    {
        if (labels.Length <= 1)
        {
            return structure.Fields.Select(field => new CompletionItem
            {
                Label = field.Identifier,
                Detail = field.Type.ToString(),
                Documentation = string.Empty,
                InsertText = field.Identifier,
                InsertTextFormat = InsertTextFormat.PlainText,
                Kind = CompletionItemKind.Field
            }).ToArray();
        }

        var currLabel = labels[0].Remove(labels[0].IndexOf('['));
        if (structure.Fields
                .FirstOrDefault(field => field.Identifier.Equals(currLabel, StringComparison.OrdinalIgnoreCase))
                is not KarelField field)
        {
            return [];
        }

        return field.Type as KarelDataType switch
        {
            KarelTypeName typeName => TraverseIfStructure(labels[1..], typeName, structures),
            KarelTypeArray arrayType => arrayType.Type switch
            {
                KarelTypeName typeName => TraverseIfStructure(labels[1..], typeName, structures),
                _ => []
            },
            _ => []
        };
    }
}
