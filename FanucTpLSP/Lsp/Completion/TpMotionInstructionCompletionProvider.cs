using FanucTpLsp.Lsp.State;
using TPLangParser.TPLang;
using TPLangParser.TPLang.Instructions;

namespace FanucTpLsp.Lsp.Completion;

public class TpMotionInstructionCompletionProvider : ICompletionProvider
{
    private static readonly string[] MotionTypes = ["J", "L", "C", "A", "S"];
    private static readonly string[] SpeedUnits = ["%", "sec", "inch/min", "deg/sec", "mm/sec", "cm/min", "WELD_SPEED"];
    private static readonly string[] TerminationTypes = ["FINE", "CNT", "CD"];
    private static readonly string[] MotionOptions =
    [
        "Wjnt", "ACC", "PTH", "AP_LD", "RT_LD", "BREAK", "Offset", "Tool_Offset",
            "ORNT_BASE", "RTCP", "SkipJump", "TIME BEFORE", "TIME AFTER", "DISTANCE BEFORE",
            "Arc Start", "Arc End", "TA_REF", "COORD", "EV", "Ind.EV", "FPLIN", "INC", "Skip"
    ];

    // Main completion method
    public CompletionItem[] GetCompletions(TpProgram program, string lineText, int column, LspServerState serverState)
    {
        var prefix = lineText[..column];
        var tokens = CompletionProviderUtils.TokenizeInput(prefix);

        return GetContextSensitiveCompletions(tokens, prefix);
    }


    private static CompletionItem[] GetContextSensitiveCompletions(List<string> tokens, string prefix)
    {
        if (tokens.Count == 0 || string.IsNullOrWhiteSpace(prefix))
        {
            // At the beginning of the line, suggest motion types
            return GetMotionTypeCompletions();
        }

        // Check if we're in a specific context
        if (!IsMotionTypeToken(tokens.FirstOrDefault()!))
        {
            return [];
        }

        if (tokens.Count == 1)
        {
            // After motion type, suggest position registers
            return GetPositionCompletions();
        }

        // If we have a circular motion, we need two positions
        if ((tokens[0] == "C" || tokens[0] == "A") && tokens.Count == 2)
        {
            return GetPositionCompletions();
        }

        // Check if we need speed suggestion
        if (IsPositionToken(tokens[^1]) && !ContainsSpeedToken(tokens))
        {
            return GetSpeedCompletions();
        }

        // Check if we need termination suggestion
        if (IsSpeedToken(tokens[^1]) && !ContainsTerminationToken(tokens))
        {
            return GetTerminationCompletions();
        }

        // After termination, suggest options
        return ContainsTerminationToken(tokens) ? GetMotionOptionCompletions(tokens) :
            // Default to an empty list if we can't determine the context
            [];
    }

    private static bool IsMotionTypeToken(string token)
        => !string.IsNullOrEmpty(token) && MotionTypes.Contains(token);

    private static bool IsPositionToken(string token)
        => !string.IsNullOrEmpty(token)
            && (token.StartsWith($"{TpPosition.Keyword}[")
                || token.StartsWith($"{TpPositionRegister.Keyword}["));

    private static bool IsSpeedToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        if (SpeedUnits.Any(token.EndsWith))
        {
            return true;
        }

        // Check for register-based speed
        return token.StartsWith($"{TpRegister.Keyword}[")
            || token.StartsWith($"{TpArgumentRegister.Keyword}[")
            || token.StartsWith("WELD_SPEED");

    }

    private static bool ContainsSpeedToken(List<string> tokens)
        => tokens.Any(IsSpeedToken);

    private static bool IsTerminationToken(string token)
        => token switch
        {
            null => false,
            "FINE" => true,
            _ => TerminationTypes.Any(token.StartsWith)
        };

    private static bool ContainsTerminationToken(List<string> tokens)
        => tokens.Any(IsTerminationToken);

    private static CompletionItem[] GetMotionTypeCompletions()
        =>
        [
            new ()
                {
                    Label = "J",
                    Kind = CompletionItemKind.Keyword,
                    Detail = "Joint motion",
                    Documentation = "Joint motion - moves all axes to arrive simultaneously at destination",
                    InsertText = "J"
                },
                new ()
                {
                    Label = "L",
                    Kind = CompletionItemKind.Keyword,
                    Detail = "Linear motion",
                    Documentation = "Linear motion - moves in a straight line to destination",
                    InsertText = "L"
                },
                new ()
                {
                    Label = "C",
                    Kind = CompletionItemKind.Keyword,
                    Detail = "Circular motion",
                    Documentation = "Circular motion - moves in a circular path through two points",
                    InsertText = "C"
                },
                new ()
                {
                    Label = "A",
                    Kind = CompletionItemKind.Keyword,
                    Detail = "Circular Arc motion",
                    Documentation = "Circular Arc motion - moves in an arc through two points",
                    InsertText = "A"
                },
                new ()
                {
                    Label = "S",
                    Kind = CompletionItemKind.Keyword,
                    Detail = "Spline motion",
                    Documentation = "Spline motion - moves in a smooth curve through specified points",
                    InsertText = "S"
                }
        ];

    private static CompletionItem[] GetPositionCompletions()
        =>
        [
            new ()
                {
                    Label = $"{TpPosition.Keyword}[n]",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Position",
                    Documentation = "A taught position",
                    InsertText = $"{TpPosition.Keyword}[$1]",
                    InsertTextFormat = InsertTextFormat.Snippet
                },
                new ()
                {
                    Label = $"{TpPositionRegister.Keyword}[{TpRegister.Keyword}[n]]",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Indirect Position Register",
                    Documentation = "Position register referenced by register value",
                    InsertText = $"{TpPositionRegister.Keyword}[{TpRegister.Keyword}[$1]]",
                    InsertTextFormat = InsertTextFormat.Snippet
                },
                new ()
                {
                    Label = $"{TpPositionRegister.Keyword}[n]",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Position Register",
                    Documentation = "Position register containing a position",
                    InsertText = $"{TpPositionRegister.Keyword}[$1]",
                    InsertTextFormat = InsertTextFormat.Snippet
                }
        ];

    private static CompletionItem[] GetSpeedCompletions()
        =>
        [
            new ()
                {
                    Label = "n%",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Percentage speed",
                    Documentation = "Speed as a percentage of maximum speed",
                    InsertText = "${1:20}%",
                    InsertTextFormat = InsertTextFormat.Snippet
                },
                new ()
                {
                    Label = "n mm/sec",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "mm/sec speed",
                    Documentation = "Speed in millimeters per second",
                    InsertText = "${1:200}mm/sec",
                    InsertTextFormat = InsertTextFormat.Snippet
                },
                new ()
                {
                    Label = "n cm/min",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "cm/min speed",
                    Documentation = "Speed in centimeters per minute",
                    InsertText = "${1:100}cm/min",
                    InsertTextFormat = InsertTextFormat.Snippet
                },
                new ()
                {
                    Label = "n sec",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Time in seconds",
                    Documentation = "Time to complete the motion in seconds",
                    InsertText = "$1sec",
                    InsertTextFormat = InsertTextFormat.Snippet
                },
                new ()
                {
                    Label = $"{TpRegister.Keyword}[n]",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Register speed",
                    Documentation = "Speed from register value",
                    InsertText = $"{TpRegister.Keyword}[$1]",
                    InsertTextFormat = InsertTextFormat.Snippet
                },
                new ()
                {
                    Label = "WELD_SPEED",
                    Kind = CompletionItemKind.Keyword,
                    Detail = "Welding speed",
                    Documentation = "Uses the speed defined in the welding schedule"
                }
        ];

    private static CompletionItem[] GetTerminationCompletions()
        =>
        [
            new ()
                {
                    Label = "FINE",
                    Kind = CompletionItemKind.Keyword,
                    Detail = "Fine termination",
                    Documentation = "Stops at exact position before starting next motion",
                    InsertText = "FINE"
                },
                new ()
                {
                    Label = "CNTn",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Continuous termination",
                    Documentation = "Continuous path with specified value (0-100)",
                    InsertText = "CNT${1:50}",
                    InsertTextFormat = InsertTextFormat.Snippet
                },
                new ()
                {
                    Label = "CDn",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Continuous distance",
                    Documentation = "Continuous path with specified distance",
                    InsertText = "CD$1",
                    InsertTextFormat = InsertTextFormat.Snippet
                }
        ];

    private static CompletionItem[] GetMotionOptionCompletions(List<string> tokens)
    {
        // Don't suggest motion options when entering arguments
        if (tokens.Last().EndsWith("[]") || tokens.Last().EndsWith("[]"))
        {
            return [];
        }

        var completions = new List<CompletionItem>();
        var existingOptions = tokens.Where(t => MotionOptions.Any(t.StartsWith)).ToList();

        // Add options that aren't already used
        if (!existingOptions.Any(o => o.StartsWith(TpWristJointOption.Keyword)))
        {
            completions.Add(new()
            {
                Label = TpWristJointOption.Keyword,
                Kind = CompletionItemKind.Keyword,
                Detail = "Wrist Joint",
                Documentation = "Specifies wrist joint motion",
                InsertText = TpWristJointOption.Keyword
            });
        }
        if (!existingOptions.Any(o => o.StartsWith(TpAccOption.Keyword)))
        {
            completions.Add(new()
            {
                Label = $"{TpAccOption.Keyword}n",
                Kind = CompletionItemKind.Snippet,
                Detail = "Acceleration",
                Documentation = "Sets acceleration percentage (1-100)",
                InsertText = $"{TpAccOption.Keyword}$0",
                InsertTextFormat = InsertTextFormat.Snippet
            });
        }
        if (!existingOptions.Any(o => o.StartsWith(TpPathOption.Keyword)))
        {
            completions.Add(new()
            {
                Label = TpPathOption.Keyword,
                Kind = CompletionItemKind.Keyword,
                Detail = "Path priority option",
                Documentation = "Enables path priority mode",
                InsertText = TpPathOption.Keyword
            });
        }
        if (!existingOptions.Any(o => o.StartsWith(TpBreakOption.Keyword)))
        {
            completions.Add(new()
            {
                Label = TpBreakOption.Keyword,
                Kind = CompletionItemKind.Keyword,
                Detail = "Break",
                Documentation = "Breaks continuous motion",
                InsertText = TpBreakOption.Keyword
            });
        }
        if (!existingOptions.Any(o => o.StartsWith(TpOffsetOption.Keyword)))
        {
            completions.Add(new()
            {
                Label = TpOffsetOption.Keyword,
                Kind = CompletionItemKind.Keyword,
                Detail = "Offset",
                Documentation = "Applies an offset to the motion, requires OFFSET CONDITION",
                InsertText = TpOffsetOption.Keyword
            });
            completions.Add(new()
            {
                Label = $"{TpOffsetOption.Keyword},{TpPositionRegister.Keyword}[n]",
                Kind = CompletionItemKind.Snippet,
                Detail = "Offset with position register",
                Documentation = "Applies an offset from position register",
                InsertText = $"{TpOffsetOption.Keyword},{TpPositionRegister.Keyword}[$0]",
                InsertTextFormat = InsertTextFormat.Snippet
            });
        }
        if (!existingOptions.Any(o => o.StartsWith(TpToolOffsetOption.Keyword)))
        {
            completions.Add(new()
            {
                Label = TpToolOffsetOption.Keyword,
                Kind = CompletionItemKind.Keyword,
                Detail = "Tool offset",
                Documentation = "TODO",
                InsertText = TpToolOffsetOption.Keyword
            });
            completions.Add(new()
            {
                Label = $"{TpToolOffsetOption.Keyword},{TpPositionRegister.Keyword}[n]",
                Kind = CompletionItemKind.Snippet,
                Detail = "Tool offset with position register",
                Documentation = "TODO",
                InsertText = $"{TpToolOffsetOption.Keyword},{TpPositionRegister.Keyword}[$0]",
                InsertTextFormat = InsertTextFormat.Snippet
            });
        }
        if (!existingOptions.Any(o => o.StartsWith(TpRemoteTcpOption.Keyword)))
        {
            completions.Add(new()
            {
                Label = TpRemoteTcpOption.Keyword,
                Kind = CompletionItemKind.Keyword,
                Detail = "Remote TCP",
                Documentation = "Enables Remote Tool Center Point mode",
                InsertText = TpRemoteTcpOption.Keyword
            });
        }
        if (!existingOptions.Any(o => o.StartsWith(TpSkipOption.Keyword)))
        {
            completions.Add(new()
            {
                Label = $"{TpSkipOption.Keyword},{TpLabel.Keyword}[n]",
                Kind = CompletionItemKind.Snippet,
                Detail = "Skip",
                Documentation = "Skip motion on condition, goto LBL[n] if motion finished.",
                InsertText = $"{TpSkipOption.Keyword},{TpLabel.Keyword}[$0]",
                InsertTextFormat = InsertTextFormat.Snippet
            });
        }
        if (!existingOptions.Any(o => o.StartsWith(TpSkipJumpOption.Keyword)))
        {
            completions.Add(new()
            {
                Label = $"{TpSkipJumpOption.Keyword},{TpLabel.Keyword}[n]",
                Kind = CompletionItemKind.Snippet,
                Detail = "SkipJump",
                Documentation = "Skip motion on condition, goto LBL[n] if motion skipped (requires option).",
                InsertText = $"{TpSkipJumpOption.Keyword},{TpLabel.Keyword}[$0]",
                InsertTextFormat = InsertTextFormat.Snippet
            });
        }
        if (!existingOptions.Any(o => o.StartsWith(TpLinearDistanceOption.ApproachKeyword)))
        {
            completions.Add(new()
            {
                Label = $"{TpLinearDistanceOption.ApproachKeyword}n",
                Kind = CompletionItemKind.Snippet,
                Detail = "Linear approach distance",
                Documentation = "Force linear approach on joint motion.",
                InsertText = $"{TpLinearDistanceOption.ApproachKeyword}$0",
                InsertTextFormat = InsertTextFormat.Snippet
            });
        }
        if (!existingOptions.Any(o => o.StartsWith(TpLinearDistanceOption.RetractKeyword)))
        {
            completions.Add(new()
            {
                Label = $"{TpLinearDistanceOption.RetractKeyword}n",
                Kind = CompletionItemKind.Snippet,
                Detail = "Linear retract distance",
                Documentation = "Force linear retract on joint motion.",
                InsertText = $"{TpLinearDistanceOption.RetractKeyword}$0",
                InsertTextFormat = InsertTextFormat.Snippet
            });
        }
        if (!existingOptions.Any(o => o.StartsWith(TpWeldOption.Keyword)))
        {
            completions.Add(new()
            {
                Label = $"{TpWeldOption.Keyword} Start[...]",
                Kind = CompletionItemKind.Snippet,
                Detail = "Arc start",
                Documentation = "Start welding arc",
                InsertText = $"{TpWeldOption.Keyword} Start[$0]",
                InsertTextFormat = InsertTextFormat.Snippet
            });
            completions.Add(new()
            {
                Label = $"{TpWeldOption.Keyword} End[...]",
                Kind = CompletionItemKind.Snippet,
                Detail = "Arc end",
                Documentation = "Stop welding arc",
                InsertText = $"{TpWeldOption.Keyword} End[$0]",
                InsertTextFormat = InsertTextFormat.Snippet
            });
            completions.Add(new()
            {
                Label = $"{TpWeldOption.Keyword} StartE<n>[...]",
                Kind = CompletionItemKind.Snippet,
                Detail = "Arc start with equipment number",
                Documentation = "Start welding arc",
                InsertText = $"{TpWeldOption.Keyword} StartE$1[$2]",
                InsertTextFormat = InsertTextFormat.Snippet
            });
            completions.Add(new()
            {
                Label = $"{TpWeldOption.Keyword} EndE<n>[...]",
                Kind = CompletionItemKind.Snippet,
                Detail = "Arc end with equipment number",
                Documentation = "Stop welding arc",
                InsertText = $"{TpWeldOption.Keyword} EndE$1[$2]",
                InsertTextFormat = InsertTextFormat.Snippet
            });
        }
        if (!existingOptions.Any(o => o.StartsWith(TpCoordMotionOption.Keyword)))
        {
            completions.Add(new()
            {
                Label = TpCoordMotionOption.Keyword,
                Kind = CompletionItemKind.Keyword,
                Detail = "Coordinate motion",
                Documentation = "Enables coordinate motion on the instruction",
                InsertText = TpCoordMotionOption.Keyword
            });
        }


        return completions.ToArray();
    }
}
