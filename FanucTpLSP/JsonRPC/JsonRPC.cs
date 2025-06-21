using System.Text.Json;
using System.Text.Json.Serialization;
using FanucTpLsp.Lsp;
using Sprache;

namespace FanucTpLsp.JsonRPC;

/// <summary>
/// JSON-RPC and LSP error codes
/// </summary>
public enum ErrorCodes
{
    ParseError = -32700,
    InvalidRequest = -32600,
    MethodNotFound = -32601,
    InvalidParams = -32602,
    InternalError = -32603,

    ServerErrorStart = -32099,
    ServerErrorEnd = -32000,

    RequestCancelled = -32800,
    ContentModified = -32801
}

/// <summary>
/// Base message interface following JSON-RPC 2.0 spec
/// </summary>
public abstract class Message
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";
}

/// <summary>
/// Request message sent from client to server
/// </summary>
public class RequestMessage : Message
{
    [JsonPropertyName("id")]
    public int Id { get; set; } = 0;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;
}

/// <summary>
/// Response message sent from server to client
/// </summary>
[JsonDerivedType(typeof(InitializeResponse))]
[JsonDerivedType(typeof(TextDocumentHoverResponse))]
[JsonDerivedType(typeof(TextDocumentDefinitionResponse))]
[JsonDerivedType(typeof(TextDocumentCodeActionResponse))]
[JsonDerivedType(typeof(TextDocumentCompletionResponse))]
[JsonDerivedType(typeof(PublishDiagnosticsNotification))]
[JsonDerivedType(typeof(ResponseError))]
public class ResponseMessage : Message
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
}

/// <summary>
/// Error response object
/// </summary>
public class ResponseError : ResponseMessage
{
    [JsonPropertyName("code")]
    public ErrorCodes Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

public class JsonRpcEncoder
{
    public static string Encode(ResponseMessage message)
    {
        if (message == null)
        {
            throw new JsonRpcException(ErrorCodes.InternalError, "Null message cannot be encoded.");
        }

        var buf = JsonSerializer.Serialize(message);
        return $"Content-Length: {buf.Length}\r\n\r\n{buf}";
    }
}

public class JsonRpcDecoder
{
    public static Parser<int> HeaderParser =
        from ident in Parse.String("Content-Length: ")
        from contentLength in Parse.Number.Select(int.Parse)
        select contentLength;

    public static TMessage? Decode<TMessage>(string json)
        => JsonSerializer.Deserialize<TMessage>(json);
}

public class JsonRpcException(ErrorCodes code, string message) : Exception(message)
{
    public ErrorCodes Code { get; set; } = code;
}
