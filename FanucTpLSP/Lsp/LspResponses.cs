using System.Text.Json.Serialization;
using JsonRPC;

namespace FanucTpLSP.Lsp.Responses;

public class ServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string? Version { get; set; } = string.Empty;
}

public class ServerCapabilities
{
}

public class InitializeResult : ResponseMessage
{
    /// <summary>
    /// Gets or sets the capabilities of the server.
    /// </summary>
    [JsonPropertyName("capabilities")]
    public ServerCapabilities Capabilities { get; set; } = new ServerCapabilities();

}
