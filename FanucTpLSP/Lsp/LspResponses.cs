using System.Text.Json.Serialization;
using FanucTpLsp.JsonRPC;

namespace FanucTpLsp.Lsp;

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

public class InitializeResult
{
    /// <summary>
    /// Gets or sets the capabilities of the server.
    /// </summary>
    [JsonPropertyName("capabilities")]
    public ServerCapabilities Capabilities { get; set; } = new();

    /// <summary>
    /// Gets or sets the information of the server.
    /// </summary>
    [JsonPropertyName("serverInfo")]
    public ServerInfo ServerInfo { get; set; } = new();
}

public class InitializeResponse : ResponseMessage
{
    /// <summary>
    /// Gets or sets the result of the initialize request.
    /// </summary>
    [JsonPropertyName("result")]
    public InitializeResult Result { get; set; } = new();
}
