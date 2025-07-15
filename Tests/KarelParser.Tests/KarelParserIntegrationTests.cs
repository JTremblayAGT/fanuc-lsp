using System.Text;

namespace KarelParser.Tests;

public class KarelParserIntegrationTests
{
    [Theory]
    [DirectoryData(@"%UserProfile%\Projects\BLK-RBT-Fanuc\KAREL\KL")]
    public void Parse_AllValidPrograms_ShouldSucceed(string filePath)
    {
        var buffer = File.ReadAllText(filePath, Encoding.ASCII);
        var result = KarelProgram.ProcessAndParse(buffer);
        Assert.True(result.WasSuccessful, $"Failed to parse valid program {Path.GetFileName(filePath)}: {result.Message}, [{result.Remainder.Line}:{result.Remainder.Column}]");
    }
}
