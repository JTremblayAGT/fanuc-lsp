using System.Text;

namespace KarelParser.Tests;

public class KarelParserIntegrationTests
{
    [Theory]
    [DirectoryData(@"%UserProfile%\Projects\BLK-RBT-Fanuc\KAREL\KL")]
    public void Parse_AllValidPrograms_ShouldSucceed(string filePath)
    {
        var buffer = File.ReadAllText(filePath, Encoding.ASCII);
        var result = KarelProgram.ProcessAndParse(filePath);
        if (result.WasSuccessful)
        {
            return;
        }

        // Build the offending-line snippet only on failure, and guard the
        // end-of-file case where IndexOf returns -1 (no trailing newline).
        var source = result.Remainder.Source;
        var position = result.Remainder.Position;
        var lineEnd = source.IndexOf("\n", position);
        var snippet = source[position..(lineEnd < 0 ? source.Length : lineEnd)];
        Assert.Fail($"Failed to parse valid program {filePath}: {result.Message}, [{result.Remainder.Line}:{result.Remainder.Column}]\n{snippet}");
    }
}
