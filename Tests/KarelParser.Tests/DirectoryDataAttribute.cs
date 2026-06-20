using System.Reflection;
using Xunit.Sdk;

namespace KarelParser.Tests;
internal class DirectoryDataAttribute(string directory) : DataAttribute
{

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var expanded = Environment.ExpandEnvironmentVariables(directory);
        if (!Directory.Exists(expanded))
        {
            Console.Error.WriteLine($"Directory not found: {expanded}");
            return [];
        }

        var files = Directory.GetFiles(expanded, "*.kl", SearchOption.AllDirectories);
        return files.Select(fileStr => new object[] { fileStr });
    }
}
