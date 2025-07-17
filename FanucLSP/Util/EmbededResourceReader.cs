using System.Reflection;
using System.Text.Json;
using FanucLsp.Lsp.Completion;

namespace FanucLSP.Util;
public class EmbeddedResourceReader
{
    public static Dictionary<string, CodeSnippet>? GetKarelBuiltInSnippets()
    {
        var assembly = Assembly.GetExecutingAssembly();
        // The resource name is based on the project's default namespace and the file's path.
        const string resourceName = "FanucLSP.Resources.Karel.karelbuiltin.code-snippets";

        using var stream = assembly.GetManifestResourceStream(resourceName);

        return stream switch
        {
            null => [],
            not null => JsonSerializer.Deserialize<Dictionary<string, CodeSnippet>>
                (new StreamReader(stream).ReadToEnd())
        };
    }
}