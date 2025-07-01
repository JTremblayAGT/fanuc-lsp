using System.Text.RegularExpressions;

namespace FanucTpLsp.Lsp.Format;

public partial class TpFormatter : IFormatter
{
    [GeneratedRegex(@"\s*\d+:\s*(.*)\s*;")]
    private static partial Regex LineContentRegex();

    [GeneratedRegex(@"^\s*([JLCS])\s+")]
    private static partial Regex MotionInstructionRegex();

    [GeneratedRegex("IF\\s+.+THEN")]
    private static partial Regex IfThenRegex();

    [GeneratedRegex("FOR\\s+.+")]
    private static partial Regex ForRegex();

    [GeneratedRegex("ENDIF|ENDFOR")]
    private static partial Regex EndBlockRegex();

    [GeneratedRegex("ELSE\\s*;")]
    private static partial Regex ElseRegex();

    [GeneratedRegex("SELECT\\s+")]
    private static partial Regex SelectRegex();

    [GeneratedRegex("=\\s*[^,]+,")]
    private static partial Regex SelectCaseRegex();

    [GeneratedRegex("^\\s*ELSE,")]
    private static partial Regex SelectElseRegex();

    private int _indentLevel = 0;

    public string Format(string content, FormattingOptions options)
    {
        var output = new StringWriter();

        var sections = content.Split("/MN", 2);

        if (sections.Length != 2)
        {
            return string.Empty;
        }
        var header = sections.First();
        output.Write(header);
        output.WriteLine("/MN");

        var rest = sections.Last().Split("/POS", 2);

        // First pass: Calculate SELECT statement alignment positions
        var selectBlocks = FindSelectBlocks(rest.First().Split('\n'));
        _indentLevel = 0;

        // Second pass: Format lines
        rest.First()
            .Split("\r\n")
            .Select(line => FormatMainLine(line, options, selectBlocks))
            .ToList()
            .ForEach(line => output.WriteLine(line));

        if (rest.Length != 2)
        {
            return output.ToString();
        }

        output.WriteLine("/POS");
        output.Write(rest.Last());

        return output.ToString();
    }

    private static Dictionary<int, SelectInfo> FindSelectBlocks(string[] lines)
    {
        var selectBlocks = new Dictionary<int, SelectInfo>();
        int? currentSelectStart = null;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var match = LineContentRegex().Match(line);
            var content = match.Success ? match.Groups[1].Value : line;

            if (SelectRegex().IsMatch(content))
            {
                currentSelectStart = i;
                selectBlocks[i] = new SelectInfo
                {
                    Cases = [],
                    MaxEqualsPos = 0,
                    MaxCommaPos = 0
                };
            }

            if (!currentSelectStart.HasValue)
            {
                continue;
            }

            // Check if this is an ELSE branch
            if (SelectElseRegex().IsMatch(content))
            {
                selectBlocks[currentSelectStart.Value].ElseCase = i;
            }
            // Check if this is a regular case with equals sign
            else if (SelectCaseRegex().IsMatch(content))
            {
                var equalsPos = content.IndexOf('=');
                var commaPos = content.IndexOf(',');

                if (equalsPos > 0 && equalsPos > selectBlocks[currentSelectStart.Value].MaxEqualsPos)
                {
                    selectBlocks[currentSelectStart.Value].MaxEqualsPos = equalsPos;
                }

                if (commaPos > 0 && commaPos > selectBlocks[currentSelectStart.Value].MaxCommaPos)
                {
                    selectBlocks[currentSelectStart.Value].MaxCommaPos = commaPos;
                }

                selectBlocks[currentSelectStart.Value].Cases.Add(i);
            }
            // If it's not a case or ELSE and not a blank line, we're outside the SELECT
            else if (!string.IsNullOrWhiteSpace(content) && !SelectRegex().IsMatch(content))
            {
                currentSelectStart = null;
            }
        }

        return selectBlocks;
    }

    private string FormatMainLine(string line, FormattingOptions options, Dictionary<int, SelectInfo> selectBlocks)
    {
        var match = LineContentRegex().Match(line);
        var content = match.Success ? match.Groups[1].Value.Trim() : line;

        if (string.IsNullOrWhiteSpace(content))
        {
            return line;
        }

        // Handle indentation for control structures that decrease indent
        if (EndBlockRegex().IsMatch(content))
        {
            _indentLevel = Math.Max(0, _indentLevel - 1);
        }

        // Motion instructions don't have any extra spacing after line start
        var spacing = MotionInstructionRegex().Match(content) switch
        {
            { Success: true } => 0,
            _ => 2
        };

        // Format the line with proper indentation
        string formattedLine;

        // Find if this is a SELECT case line for alignment
        var isSelectCase = false;
        var isElseBranch = false;
        SelectInfo? selectInfo = null;

        foreach (var kvp in selectBlocks)
        {
            if (kvp.Value.Cases.Contains(line.GetHashCode()))
            {
                isSelectCase = true;
                selectInfo = kvp.Value;
                break;
            }
            else if (kvp.Value.ElseCase.HasValue && kvp.Value.ElseCase.Value == line.GetHashCode())
            {
                isElseBranch = true;
                selectInfo = kvp.Value;
                break;
            }
        }

        // Apply SELECT case alignment if applicable
        if (isSelectCase && selectInfo != null)
        {
            var equalsPos = content.IndexOf('=');
            if (equalsPos >= 0)
            {
                var neededSpaces = selectInfo.MaxEqualsPos - equalsPos;
                if (neededSpaces > 0)
                {
                    content = content.Insert(equalsPos, new string(' ', neededSpaces));
                }
            }
            formattedLine = $"   1:{new string(' ', spacing + _indentLevel * options.TabSize)}{content} ;";
        }
        // Apply ELSE branch alignment if applicable
        else if (isElseBranch && selectInfo != null)
        {
            var commaPos = content.IndexOf(',');
            if (commaPos >= 0 && selectInfo.MaxCommaPos > 0)
            {
                var neededSpaces = selectInfo.MaxCommaPos - commaPos;
                if (neededSpaces > 0)
                {
                    content = new string(' ', neededSpaces) + content;
                }
            }
            formattedLine = $"   1:{new string(' ', spacing + _indentLevel * options.TabSize)}{content} ;";
        }
        // Special case for ELSE (maintains same indentation as its IF)
        else if (ElseRegex().IsMatch(content))
        {
            var indentLevel = Math.Max(0, _indentLevel - 1);
            formattedLine = $"   1:{new string(' ', spacing + indentLevel * options.TabSize)}ELSE ;";
        }
        else
        {
            formattedLine = $"   1:{new string(' ', spacing + _indentLevel * options.TabSize)}{content} ;";
        }

        // Check for structures that increase indentation
        if (IfThenRegex().IsMatch(content) || ForRegex().IsMatch(content))
        {
            _indentLevel++;
        }

        return formattedLine;
    }

    private class SelectInfo
    {
        public List<int> Cases { get; set; } = new();
        public int? ElseCase { get; set; }
        public int MaxEqualsPos { get; set; }
        public int MaxCommaPos { get; set; }
    }
}
