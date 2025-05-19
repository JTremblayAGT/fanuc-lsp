using System.Text.Json;
using System.Text.Json.Serialization;

using Sprache;

namespace JsonRPC
{
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

    public class Methods
    {
        public const string Initialize = "initialize";
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
    /// Error response object
    /// </summary>
    public class ResponseError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public object? Data { get; set; }
    }

    /// <summary>
    /// Response message sent from server to client
    /// </summary>
    public class ResponseMessage : Message
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
    }

    /// <summary>
    /// Notification message (without an id)
    /// </summary>
    public class NotificationMessage : Message
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;
    }

    public class JsonRpcEncoder
    {
        public static string Encode<TMessage>(TMessage request)
        {
            var buf = JsonSerializer.Serialize(request);
            return $"Content-Length: {buf.Length}\r\n\r\n{buf}";
        }

    }

    public class JsonRpcDecoder
    {
        private static Parser<int> HeaderParser =
            from ident in Parse.String("Content-Length: ")
            from contentLength in Parse.Number.Select(int.Parse)
            from separator in Parse.String("\r\n\r\n")
            select contentLength;

        public static TMessage? Decode<TMessage>(string json)
        {
            var result = HeaderParser.TryParse(json);
            if (!result.WasSuccessful)
            {
                throw new JsonRpcException(ErrorCodes.ParseError, "Failed to parse reponse header.");
            }
            var contentLength = result.Value;
            var response = JsonSerializer.Deserialize<TMessage>(json.Substring(result.Remainder.Position, contentLength));
            return response;
        }
    }

    public class JsonRpcException : Exception
    {
        public ErrorCodes Code { get; set; }

        public JsonRpcException(ErrorCodes code, string message) : base(message)
        {
            Code = code;
        }
    }
}
