using System.Text.Json.Serialization;
using JsonRPC;

namespace FanucTpLSP.Lsp;


public class ClientInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string? Version { get; set; } = string.Empty;
}

public class InitializeParams
{
    /// <summary>
    /// Gets or sets the information of the client.
    /// </summary>
    [JsonPropertyName("clientInfo")]
    public ClientInfo? ClientInfo { get; set; } = new ClientInfo();

    /// <summary>
    /// Gets or sets the capabilities of the client.
    /// </summary>
    //[JsonPropertyName("capabilities")]
    //public ClientCapabilities Capabilities { get; set; } = new ClientCapabilities();
}

/// <summary>
/// Represents a request to the Language Server Protocol (LSP).
/// </summary>
public class InitializeRequest : RequestMessage
{
    /// <summary>
    /// Gets or sets the parameters for the initialize request.
    /// </summary>
    [JsonPropertyName("params")]
    public InitializeParams Params { get; set; } = new InitializeParams();
}
