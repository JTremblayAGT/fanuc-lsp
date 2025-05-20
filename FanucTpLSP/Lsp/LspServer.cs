using FanucTpLsp.JsonRPC;
using FanucTpLsp.Lsp.State;

namespace FanucTpLsp.Lsp;

public class LspServer(string logFilePath)
{
    private LspServerState _state = new();

    public bool Initialize()
    {
        _state.IsInitialized = true;

        return _state.IsInitialized;
    }

    // TODO: returning null is fine but need to throw exceptions if request failed
    public ResponseMessage? HandleRequest(string method, string json)
        => method switch
        {
            LspMethods.Initialize => HandleInitializeRequest(json),
            LspMethods.Initialized => HandleInitializedNotification(json),

            LspMethods.TextDocumentDidOpen => HandleTextDocumentDidOpen(json),
            LspMethods.TextDocumentDidClose => HandleTextDocumentDidClose(json),
            LspMethods.TextDocumentDidChange => HandleTextDocumentDidChange(json),
            LspMethods.TextDocumentDidHover => HandleTextDocumentDidHover(json),

            LspMethods.Shutdown => HandleShutdownRequest(),
            _ => null
        };

    private InitializeResponse? HandleInitializeRequest(string json)
    {
        var request = JsonRpcDecoder.Decode<InitializeRequest>(json);
        if (request == null)
        {
            throw new JsonRpcException(ErrorCodes.ParseError, "Failed to decode InitializeRequest");
        }

        var initialized = Initialize();

        // Handle the initialize request and return the result
        var response = new InitializeResponse
        {
            Id = request.Id,
            Result = new InitializeResult
            {
                Capabilities = new()
                {
                    TextDocumentSync = new TextDocumentSyncOptions
                    {
                        Change = TextDocumentSyncKind.Incremental,
                        OpenClose = true
                    },
                    HoverProvider = true
                },
                ServerInfo = new()
                {
                    Name = "fanuctp-lsp",
                    Version = "0.0.0-alpha1"
                }
            }
        };


        return initialized ? response : null;
    }

    private ResponseMessage? HandleInitializedNotification(string json)
    {
        var notification = JsonRpcDecoder.Decode<RequestMessage>(json);
        // Handle the initialized notification
        LogMessage($"NOTIFICATION: {notification?.Method}");

        // TODO: figure out what to do for initialization
        return null;
    }

    private ResponseMessage? HandleTextDocumentDidOpen(string json)
    {
        // Handle the text document did open notification
        var notification = JsonRpcDecoder.Decode<TextDocumentDidOpenNotification>(json);
        if (notification == null)
        {
            throw new JsonRpcException(ErrorCodes.ParseError, "Failed to decode TextDocumentDidOpenNotification");
        }
        LogMessage($"[TextDocumentDidOpen]: {notification.Params.TextDocument.Uri}");

        if (_state.OpenedTextDocuments.ContainsKey(notification.Params.TextDocument.Uri))
        {
            LogMessage($"[TextDocumentDidOpen]: Document already opened: {notification?.Params.TextDocument.Uri}");
            return null;
        }

        _state.OpenedTextDocuments.Add(notification.Params.TextDocument.Uri, notification.Params.TextDocument);

        return null;
    }

    private ResponseMessage? HandleTextDocumentDidClose(string json)
    {
        // Handle the text document did close notification
        var notification = JsonRpcDecoder.Decode<TextDocumentDidCloseNotification>(json);
        if (notification == null)
        {
            throw new JsonRpcException(ErrorCodes.ParseError, "Failed to decode TextDocumentDidCloseNotification");
        }
        LogMessage($"[TextDocumentDidClose]: {notification.Params.TextDocument.Uri}");

        if (!_state.OpenedTextDocuments.ContainsKey(notification.Params.TextDocument.Uri))
        {
            LogMessage($"[TextDocumentDidClose]: Document not opened: {notification?.Params.TextDocument.Uri}");
            return null;
        }

        _state.OpenedTextDocuments.Remove(notification.Params.TextDocument.Uri);
        return null;
    }

    private ResponseMessage? HandleTextDocumentDidChange(string json)
    {
        var notification = JsonRpcDecoder.Decode<TextDocumentDidChangeNotification>(json);
        // Handle the text document did change notification
        if (notification == null)
        {
            throw new JsonRpcException(ErrorCodes.ParseError, "Failed to decode TextDocumentDidChangeParams");
        }
        LogMessage($"[TextDocumentDidChange]: {notification.Params.TextDocument.Uri}");

        if (!_state.OpenedTextDocuments.ContainsKey(notification.Params.TextDocument.Uri))
        {
            LogMessage($"[TextDocumentDidChange]: Document not opened: {notification.Params.TextDocument.Uri}");
            return null;
        }

        // TODO: handle change (update contents)

        return null;
    }

    private TextDocumentHoverResponse? HandleTextDocumentDidHover(string json)
    {
        var request = JsonRpcDecoder.Decode<TextDocumentDidHoverRequest>(json);
        // Handle the text document did hover notification
        if (request == null)
        {
            throw new JsonRpcException(ErrorCodes.ParseError, "Failed to decode TextDocumentDidHoverNotification");
        }
        LogMessage($"[TextDocumentDidHover]: {request.Params.TextDocument.Uri}");

        if (!_state.OpenedTextDocuments.ContainsKey(request.Params.TextDocument.Uri))
        {
            LogMessage($"[TextDocumentDidHover]: Document not opened: {request.Params.TextDocument.Uri}");
            return null;
        }

        return new TextDocumentHoverResponse
        {
            Id = request.Id,
            Result = new HoverResult
            {
                Contents = new MarkupContent
                {
                    Kind = "plaintext",
                    Value = "Oi I just did the thing!"
                },
                Range = new TextDocumentContentRange
                {
                    Start = request.Params.Position,
                    End = request.Params.Position,
                }
            },
        };
    }

    private ResponseMessage? HandleShutdownRequest()
    {
        // Handle the shutdown request
        LogMessage("Server is shutting down.");
        System.Environment.Exit(0);

        // Unreachable code, but required for the method signature
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
