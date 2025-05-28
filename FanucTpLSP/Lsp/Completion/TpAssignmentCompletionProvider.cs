using TPLangParser.TPLang;

using Sprache;
using FanucTpLsp.Lsp.State;

namespace FanucTpLsp.Lsp.Completion;

internal class TpAssignmentCompletionProvider : ICompletionProvider
{
    public CompletionItem[] GetCompletions(TpProgram program, string lineText, int column, LspServerState serverState)
        => CompletionProviderUtils.TokenizeInput(lineText[..column]) switch
        {
            [string lhs, "="] => TpValue.GetParser().TryParse(lhs) switch
            {
                { WasSuccessful: true } result => result.Value switch
                {
                    TpValueRegister registerValue => registerValue.Register switch
                    {
                        TpPositionRegister posReg => PosRegCompletions(posReg),
                        TpStringRegister strReg => StringRegisterAssignmentCompletions,
                        TpRegister numReg => RegisterAssignmentCompletions,
                        _ => []
                    },
                    TpValueIOPort ioPort => IoCompletions(ioPort),
                    TpValueFlag flag => FlagAssignmentCompletions,
                    _ => []
                },
                _ => []
            },
            _ => []
        };

    private CompletionItem[] PosRegCompletions(TpPositionRegister posReg)
        => posReg.Access switch
        {
            TpAccessDirect or TpAccessIndirect => PositionRegisterAssignmentCompletions,
            TpAccessMultiple => PositionRegisterElemAssignmentCompletions,
            _ => []
        };

    private CompletionItem[] IoCompletions(TpValueIOPort iOPort)
        => iOPort.IOPort.Type switch
        {
            TpIOType.Output => iOPort.IOPort switch
            {
                // TODO: add all options
                TpDigitalIOPort digitalIo => DigitalOutCompletions,
                TpRobotIOPort robotIo => RobotOutCompletions,
                TpWeldingIOPort weldIo => WeldOutCompletions,
                _ => [],
            },
            _ => [] // Cannot assign to input
        }
        ;

    private static CompletionItem[] OnOffCompletions
        => [
            new()
            {
                Label = "ON",
                Detail = "On (true)",
                Documentation = string.Empty,
                InsertText = "ON",
                InsertTextFormat = InsertTextFormat.PlainText,
                Kind = CompletionItemKind.Constant
            },
            new()
            {
                Label = "OFF",
                Detail = "Off (false)",
                Documentation = string.Empty,
                InsertText = "OFF",
                InsertTextFormat = InsertTextFormat.PlainText,
                Kind = CompletionItemKind.Constant
            }
        ];

    private static CompletionItem[] PulseCompletions
        => [
            new()
            {
                Label = "PULSE",
                Detail = "Pulse",
                Documentation = "Pulse output",
                InsertText = "PULSE",
                InsertTextFormat = InsertTextFormat.PlainText,
                Kind = CompletionItemKind.Keyword
            },
            new()
            {
                Label = "PULSE [,width]",
                Detail = "Pulse,width ",
                Documentation = "Pulse output with specified pulse width.",
                InsertText = "PULSE,$0sec",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
        ];

    private static CompletionItem[] RegisterCompletions
        => [
            new()
            {
                Label = "R[n]",
                Detail = "Numerical register",
                Documentation = "Contains a numerical value",
                InsertText = "R[$0]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "AR[n]",
                Detail = "Argument register",
                Documentation = "Contains a numerical value",
                InsertText = "AR[$0]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "R[R[n]]",
                Detail = "Numerical register (indirect)",
                Documentation = "Contains a numerical value",
                InsertText = "R[R[$0]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "R[AR[n]]",
                Detail = "Numerical register (indirect)",
                Documentation = "Contains a numerical value",
                InsertText = "R[AR[$0]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
        ];

    private static CompletionItem[] DigitalOutCompletions
        => OnOffCompletions.Concat(PulseCompletions).Concat(RegisterCompletions).ToArray();

    private static CompletionItem[] RobotOutCompletions
        => DigitalOutCompletions;

    private static CompletionItem[] WeldOutCompletions
        => DigitalOutCompletions;

    private static CompletionItem[] DigitalInCompletions
        => [
            new()
            {
                Label = "DI[n]",
                Detail = "Digital input",
                Documentation = string.Empty,
                InsertText = "DI[$0]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "DI[R[n]]",
                Detail = "Digital input (indirect)",
                Documentation = string.Empty,
                InsertText = "DI[R[$0]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "DI[AR[n]]",
                Detail = "Digital input (indirect)",
                Documentation = string.Empty,
                InsertText = "DI[AR[$0]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
        ];

    private static CompletionItem[] RobotInCompletions
        => [
            new()
            {
                Label = "RI[n]",
                Detail = "Robot input",
                Documentation = string.Empty,
                InsertText = "DI[$0]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "RI[R[n]]",
                Detail = "Robot input (indirect)",
                Documentation = string.Empty,
                InsertText = "RI[R[$0]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "RI[AR[n]]",
                Detail = "Robot input (indirect)",
                Documentation = string.Empty,
                InsertText = "RI[AR[$0]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
        ];

    private static CompletionItem[] WeldInCompletions
        => [
            new()
            {
                Label = "WI[n]",
                Detail = "Weld input",
                Documentation = string.Empty,
                InsertText = "DI[$0]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "WI[R[n]]",
                Detail = "Weld input (indirect)",
                Documentation = string.Empty,
                InsertText = "WI[R[$0]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "WI[AR[n]]",
                Detail = "Weld input (indirect)",
                Documentation = string.Empty,
                InsertText = "WI[AR[$0]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
        ];


    // TODO: math expressions
    private static CompletionItem[] NumericalValueCompletions
        => [
            new()
            {
                Label = "(-n)",
                Detail = "Negative number constant",
                Documentation = string.Empty,
                InsertText = "(-$0)",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Constant
            },
            new()
            {
                Label = "PR[n,i]",
                Detail = "Position register element",
                Documentation = "Contains a numerical value",
                InsertText = "PR[$1,$0]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "$[<prog>]<var>",
                Detail = "Karel variable",
                Documentation = "Variable from a Karel program.",
                InsertText = "$[$1]$0",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Variable
            },
            new()
            {
                Label = "$<var>",
                Detail = "System variable",
                Documentation = "system variable",
                InsertText = "$$0",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Variable
            },
        ];

    private static CompletionItem[] RegisterAssignmentCompletions
        => RegisterCompletions
        .Concat(DigitalInCompletions)
        .Concat(NumericalValueCompletions)
        .Concat([
            new()
            {
                Label = "STRLEN SR[n]",
                Detail = "String length",
                Documentation = "Stores the length of the given string into the register.",
                InsertText = "STRLEN SR[$0]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet,
            },
            new()
            {
                Label = "STRLEN AR[n]",
                Detail = "String length",
                Documentation = "Stores the length of the given string into the register.",
                InsertText = "STRLEN AR[$0]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet,
            },
            new()
            {
                Label = "STRLEN SR[R[n]]",
                Detail = "String length (indirect)",
                Documentation = "Stores the length of the given string into the register.",
                InsertText = "STRLEN SR[R[$0]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet,
            },
            new()
            {
                Label = "STRLEN SR[AR[n]]",
                Detail = "String length (indirect)",
                Documentation = "Stores the length of the given string into the register.",
                InsertText = "STRLEN SR[AR[$0]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet,
            },
            new()
            {
                Label = "FINDSTR SR[n], SR[m]",
                Detail = "String search",
                Documentation = "Searches the first string for the value in the second string, stores index in register.",
                InsertText = "FINDSTR SR[$1], SR[$0]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet,
            },
            new()
            {
                Label = "FINDSTR AR[n], AR[m]",
                Detail = "String search",
                Documentation = "Searches the first string for the value in the second string, stores index in register.",
                InsertText = "FINDSTR AR[$1], AR[$0]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet,
            },
        ]).ToArray();

    private static CompletionItem[] PositionRegisterAssignmentCompletions
        => [
            new()
            {
                Label = "PR[n]",
                Detail = "Position register",
                Documentation = "Contains a position",
                InsertText = "PR[$1]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "PR[R[n]]",
                Detail = "Position register (indirect)",
                Documentation = "Contains a position",
                InsertText = "PR[R[$1]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "PR[AR[n]]",
                Detail = "Position register (indirect)",
                Documentation = "Contains a position",
                InsertText = "PR[AR[$1]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
        ];

    private static CompletionItem[] PositionRegisterElemAssignmentCompletions
        => RegisterAssignmentCompletions;

    private static CompletionItem[] StringRegisterAssignmentCompletions
        => [
            new()
            {
                Label = "R[n]",
                Detail = "Numerical register",
                Documentation = "Contains a numerical value",
                InsertText = "R[$0]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "AR[n]",
                Detail = "Argument register",
                Documentation = "Contains a numerical value",
                InsertText = "AR[$0]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "R[R[n]]",
                Detail = "Numerical register (indirect)",
                Documentation = "Contains a numerical value",
                InsertText = "R[R[$0]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "R[AR[n]]",
                Detail = "Numerical register (indirect)",
                Documentation = "Contains a numerical value",
                InsertText = "R[R[$0]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "SUBSTR SR[n], i, j",
                Detail = "Substring SR[i..j]",
                Documentation = "Returns the substring between the two indices.",
                InsertText = "SUBSTR SR[$1], $2, $0",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "SUBSTR AR[n], i, j",
                Detail = "Substring AR[i..j]",
                Documentation = "Returns the substring between the two indices.",
                InsertText = "SUBSTR AR[$1], $2, $0",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            }
        ];

    private static CompletionItem[] FlagAssignmentCompletions
        => OnOffCompletions.Concat([
            new()
            {
                Label = "F[n]",
                Detail = "Flag (direct access)",
                Documentation = "can be either ON or OFF",
                InsertText = "F[$0]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
            new()
            {
                Label = "F[R[n]]",
                Detail = "Flag (indirect access)",
                Documentation = "can be either ON or OFF",
                InsertText = "F[R[$0]]",
                InsertTextFormat = InsertTextFormat.Snippet,
                Kind = CompletionItemKind.Snippet
            },
        ]).ToArray();

}
