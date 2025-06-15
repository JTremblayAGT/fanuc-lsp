using System.Reflection;
using Xunit.Sdk;

namespace KarelParser.Tests;
internal class DirectoryDataAttribute(string directory) : DataAttribute
{

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        if (!Directory.Exists(directory))
        {
            Console.Error.WriteLine($"Directory not found: {directory}");
            return null!;
        }

        var files = Directory.GetFiles(directory, "*.kl", SearchOption.AllDirectories);
        return files.Select(fileStr => new object[]{fileStr});
    }
}
