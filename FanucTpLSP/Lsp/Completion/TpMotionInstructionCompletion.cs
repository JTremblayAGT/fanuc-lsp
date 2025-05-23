using System.Text.RegularExpressions;

namespace FanucTpLsp.Lsp.Completion
{
    public class TpMotionInstructionCompletion
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
        public static CompletionItem[] GetCompletions(string lineText, int column)
        {
            var prefix = lineText[..column];
            var tokens = TokenizeInput(prefix);

            return GetContextSensitiveCompletions(tokens, prefix);
        }

        // Break down the input into tokens for analysis
        private static List<string> TokenizeInput(string input)
        {
            // First, remove line number if present (format: "123: ")
            var lineWithoutNumber = RemoveLineNumber(input);

            // Simple tokenization - split by whitespace but preserve quoted strings
            var tokens = new List<string>();
            const string pattern = """
                                   [^\s"]+|"[^"]*\"
                                   """;
            var matches = Regex.Matches(lineWithoutNumber, pattern);

            foreach (Match match in matches)
            {
                tokens.Add(match.Value);
            }

            return tokens;
        }

        // Remove line number prefix if present (format: "123: ")
        private static string RemoveLineNumber(string input)
        {
            // Match pattern like "123: " at the beginning of the line
            const string lineNumberPattern = @"^\s*\d+\s*:";
            var match = Regex.Match(input, lineNumberPattern);

            return match.Success ?
                // Strip off the line number and the colon
                input[match.Value.Length..].TrimStart() : input;
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
            => !string.IsNullOrEmpty(token) && (token.StartsWith("P[") || token.StartsWith("PR["));

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
            return token.StartsWith("R[") || token.StartsWith("AR[") || token.StartsWith("WELD_SPEED");
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
                    Label = "P[n]",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Position",
                    Documentation = "A taught position",
                    InsertText = "P[$1]",
                    InsertTextFormat = InsertTextFormat.Snippet
                },
                new ()
                {
                    Label = "PR[R[n]]",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Indirect Position Register",
                    Documentation = "Position register referenced by register value",
                    InsertText = "P[R[$1]]",
                    InsertTextFormat = InsertTextFormat.Snippet
                },
                new ()
                {
                    Label = "PR[n]",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Position Register",
                    Documentation = "Position register containing a position",
                    InsertText = "PR[$1]",
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
                    Label = "R[n]",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Register speed",
                    Documentation = "Speed from register value",
                    InsertText = "R[$1]",
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
            var completions = new List<CompletionItem>();
            var existingOptions = tokens.Where(t => MotionOptions.Any(t.StartsWith)).ToList();

            // Add options that aren't already used
            if (!existingOptions.Any(o => o.StartsWith("Wjnt")))
            {
                completions.Add(new()
                {
                    Label = "Wjnt",
                    Kind = CompletionItemKind.Keyword,
                    Detail = "Wrist Joint",
                    Documentation = "Specifies wrist joint motion",
                    InsertText = "Wjnt"
                });
            }

            if (!existingOptions.Any(o => o.StartsWith("ACC")))
            {
                completions.Add(new()
                {
                    Label = "ACCn",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Acceleration",
                    Documentation = "Sets acceleration percentage (1-100)",
                    InsertText = "ACC$0",
                    InsertTextFormat = InsertTextFormat.Snippet
                });
            }

            if (!existingOptions.Any(o => o.StartsWith("PTH")))
            {
                completions.Add(new()
                {
                    Label = "PTH",
                    Kind = CompletionItemKind.Keyword,
                    Detail = "Path",
                    Documentation = "Enables path mode",
                    InsertText = "PTH"
                });
            }

            if (!existingOptions.Any(o => o.StartsWith("BREAK")))
            {
                completions.Add(new()
                {
                    Label = "BREAK",
                    Kind = CompletionItemKind.Keyword,
                    Detail = "Break",
                    Documentation = "Breaks continuous motion",
                    InsertText = "BREAK"
                });
            }

            if (!existingOptions.Any(o => o.StartsWith("Offset")))
            {
                completions.Add(new()
                {
                    Label = "Offset",
                    Kind = CompletionItemKind.Keyword,
                    Detail = "Offset",
                    Documentation = "Applies an offset to the motion",
                    InsertText = "Offset"
                });

                completions.Add(new()
                {
                    Label = "Offset, PR[n]",
                    Kind = CompletionItemKind.Snippet,
                    Detail = "Offset with position register",
                    Documentation = "Applies an offset from position register",
                    InsertText = "Offset, PR[$0]",
                    InsertTextFormat = InsertTextFormat.Snippet
                });
            }

            if (!existingOptions.Any(o => o.StartsWith("RTCP")))
            {
                completions.Add(new()
                {
                    Label = "RTCP",
                    Kind = CompletionItemKind.Keyword,
                    Detail = "Remote TCP",
                    Documentation = "Enables Remote Tool Center Point mode",
                    InsertText = "RTCP"
                });
            }

            return completions.ToArray();
        }
    }
}
