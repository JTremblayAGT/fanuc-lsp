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
            [.., { } variable] when variable.Contains('$') => CompleteVariable(variable, serverState),
            _ => []
        };

    private static CompletionItem[] CompleteVariable(string variable, LspServerState serverState)
        => variable switch
        {
            not null when Variable().IsMatch(variable) => GetVariableCompletions(variable, serverState),
            not null when ProgramName().IsMatch(variable) => CompletionProviderUtils.GetKarelProgramNames(serverState),
            _ => []
        };

    private static CompletionItem[] GetVariableCompletions(string partialVar, LspServerState serverState)
    {
        var match = Variable().Match(partialVar);
        var programName = match.Groups[1].Value;

        if (serverState.AllTextDocuments
                .FirstOrDefault(kvp => Path.GetFileNameWithoutExtension(kvp.Key)
                    .Equals(programName, StringComparison.OrdinalIgnoreCase))
                .Value.Program is not KlProgram klProg)
        {
            return [];
        }

        var labels = partialVar[(partialVar.IndexOf('=') + 1)..].Replace($"$[{programName.ToUpper()}]", string.Empty).Split('.');
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
        var currLabel = labels.First();
        if (currLabel.Contains('['))
        {
            currLabel = currLabel.Remove(currLabel.IndexOf('['));
        }
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
                if (typ.Type is not { } userType)
                {
                    return false;
                }

                return userType is KarelStructure;
            })
            .ToDictionary(typ => typ.Identifier, typ => (KarelStructure)((KarelUserType)typ.Type));

        return karelVar.Type switch
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
        var currLabel = labels.First();
        if (currLabel.Contains('['))
        {
            currLabel = currLabel.Remove(currLabel.IndexOf('['));
        }
        if (!structures.TryGetValue(typeName.Identifier, out var structure))
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

        var currLabel = labels.First();
        if (currLabel.Contains('['))
        {
            currLabel = currLabel.Remove(currLabel.IndexOf('['));
        }
        if (structure.Fields
                .FirstOrDefault(field => field.Identifier.Equals(currLabel, StringComparison.OrdinalIgnoreCase))
                is not { } field)
        {
            return [];
        }

        return field.Type switch
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
