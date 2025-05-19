using FanucTpLsp.JsonRPC;

namespace FanucTpLsp.Lsp;

public class LspServer(string logFilePath)
{
    public void Initialize()
    {
        // Register the initialize method with the JSON-RPC server
    }

    public ResponseMessage? HandleRequest(string method, string json)
        => method switch
        {
            LspMethods.Initialize => HandleInitializeRequest(JsonRpcDecoder.Decode<InitializeRequest>(json)),
            LspMethods.Initialized => HandleInitializedNotification(JsonRpcDecoder.Decode<RequestMessage>(json)),
            _ => null
        };

    private InitializeResponse HandleInitializeRequest(InitializeRequest? request)
    {
        if (request == null)
        {
            throw new JsonRpcException(ErrorCodes.ParseError, "Failed to decode InitializeRequest");
        }
        // Handle the initialize request and return the result
        var response = new InitializeResponse
        {
            Id = request.Id,
            Result = new InitializeResult
            {
                Capabilities = new()
                {

                },
                ServerInfo = new()
                {
                    Name = "fanuctp-lsp",
                    Version = "0.0.0-alpha1"
                }
            }
        };
        return response;
    }

    public ResponseMessage? HandleInitializedNotification(RequestMessage? request)
    {
        // Handle the initialized notification
        LogMessage($"NOTIFICATION: {request?.Method}");
        return null;
    }

    private void LogMessage(string message)
    {
        try
        {
            File.AppendAllText(logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error logging message: {ex.Message}");
        }
    }
}
