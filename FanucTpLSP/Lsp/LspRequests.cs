using System.Text.Json.Serialization;
using FanucTpLsp.JsonRPC;

namespace FanucTpLsp.Lsp;

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

/// <summary>
/// Represents the request that is sent by the client when shutting down
/// </summary>
public class ShutdownRequest : RequestMessage;

