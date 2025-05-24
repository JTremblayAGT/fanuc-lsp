using FanucTpLsp.JsonRPC;
using FanucTpLsp.Lsp.State;
using Sprache;
using TPLangParser.TPLang;

namespace FanucTpLsp.Lsp;

public class LspServer(string logFilePath)
{
    private LspServerState _state = new(logFilePath);

    public bool Initialize()
    {
        _state.IsInitialized = true;

        // TODO: project initialization stuff

        return _state.IsInitialized;
    }

    public ResponseMessage? HandleRequest(string method, string json)
        => method switch
        {
            LspMethods.Initialize => HandleInitializeRequest(json),
            LspMethods.Initialized => HandleInitializedNotification(json),

            LspMethods.TextDocumentDidOpen => HandleTextDocumentDidOpen(json),
            LspMethods.TextDocumentDidClose => HandleTextDocumentDidClose(json),
            LspMethods.TextDocumentDidChange => HandleTextDocumentDidChange(json),
            LspMethods.TextDocumentDidSave => HandleTextDocumentDidSave(json),
            LspMethods.TextDocumentDidHover => HandleTextDocumentDidHover(json),
            LspMethods.TextDocumentDefinition => HandleTextDocumentDefinition(json),
            LspMethods.TextDocumentCodeAction => HandleTextDocumentCodeAction(json),
            LspMethods.TextDocumentCompletion => HandleTextDocumentCompletion(json),

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
            Result = new()
            {
                Capabilities = new()
                {
                    TextDocumentSync = new()
                    {
                        Change = TextDocumentSyncKind.Incremental,
                        OpenClose = true,
                        Save = true,
                    },
                    HoverProvider = true,      // TODO: look into this
                    DefinitionProvider = true, // TODO: look into this
                    CodeActionProvider = false, // TODO: look into this
                    CompletionProvider = new()
                    {
                        TriggerCharacters =
                        [
                            " ",
                            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
                            "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
                            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
                            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
                            "[", "]"  // Also adding bracket characters for position/register triggers
                        ]
                    },
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

        var result = _state.OnDocumentOpen(notification.Params.TextDocument);

        return ParseResultToDiagnostics(result, notification.Params.TextDocument.Uri);
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

        var changedDocumentUri = notification.Params.TextDocument.Uri;
        LogMessage($"[TextDocumentDidChange]: {changedDocumentUri}");

        _state.UpdateDocumentText(changedDocumentUri, notification.Params.ContentChanges);

        return null;
    }

    private PublishDiagnosticsNotification? HandleTextDocumentDidSave(string json)
    {
        var request = JsonRpcDecoder.Decode<TextDocumentDidSaveNotification>(json);
        if (request == null)
        {
            throw new JsonRpcException(ErrorCodes.ParseError, "Failed to decode TextDocumentDidSaveNotification");
        }
        LogMessage($"[TextDocumentDidSave]: {request.Params.TextDocument.Uri}");
        if (!_state.OpenedTextDocuments.TryGetValue(request.Params.TextDocument.Uri, out var documentState))
        {
            LogMessage($"[TextDocumentDidChange]: Document not opened: {request.Params.TextDocument.Uri}");
            return null;
        }

        // Might not actually need to do anything
        var result = _state.UpdateParsedProgram(request.Params.TextDocument.Uri);

        return ParseResultToDiagnostics(result, request.Params.TextDocument.Uri);
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

        return new TextDocumentHoverResponse
        {
            Id = request.Id,
            Result = _state.GetHoverResult(request.Params.TextDocument.Uri, request.Params.Position),
        };
    }

    private TextDocumentDefinitionResponse? HandleTextDocumentDefinition(string json)
    {
        var request = JsonRpcDecoder.Decode<TextDocumentDefinitionRequest>(json);
        // Handle the text document definition request
        if (request == null)
        {
            throw new JsonRpcException(ErrorCodes.ParseError, "Failed to decode TextDocumentDefinitionRequest");
        }
        LogMessage($"[TextDocumentDefinition]: {request.Params.TextDocument.Uri}");


        // TODO: we'll have to find the program in project folder and open it in the current buffer

        return new()
        {
            Id = request.Id,
            Result = _state.GetLocation(request.Params.TextDocument.Uri, request.Params.Position),
        };
    }

    private TextDocumentCodeActionResponse? HandleTextDocumentCodeAction(string json)
    {
        var request = JsonRpcDecoder.Decode<TextDocumentCodeActionRequest>(json);
        // Handle the text document code action request
        if (request == null)
        {
            throw new JsonRpcException(ErrorCodes.ParseError, "Failed to decode TextDocumentCodeActionRequest");
        }
        LogMessage($"[TextDocumentCodeAction]: {request.Params.TextDocument.Uri}");

        if (!_state.OpenedTextDocuments.ContainsKey(request.Params.TextDocument.Uri))
        {
            LogMessage($"[TextDocumentCodeAction]: Document not opened: {request.Params.TextDocument.Uri}");
            return null;
        }

        // TODO: implement code actions (e.g. refactoring, line renumbering, syncing comments with robot, etc.)

        return new()
        {
            Id = request.Id,
            Result = []
        };
    }

    private TextDocumentCompletionResponse? HandleTextDocumentCompletion(string json)
    {
        var request = JsonRpcDecoder.Decode<TextDocumentCompletionRequest>(json);
        // Handle the text document completion request
        if (request == null)
        {
            throw new JsonRpcException(ErrorCodes.ParseError, "Failed to decode TextDocumentCompletionRequest");
        }

        return new()
        {
            Id = request.Id,
            Result = _state.GetCompletionItems()
        };
    }

    private ResponseMessage? HandleShutdownRequest()
    {
        // Handle the shutdown request
        LogMessage("Server is shutting down.");
        Environment.Exit(0);

        // Unreachable code, but required for the method signature
        return null;
    }

    private static PublishDiagnosticsNotification? ParseResultToDiagnostics(IResult<TpProgram> result, string uri)
        => result switch
        {
            { WasSuccessful: false } => new()
            {
                Params = new()
                {
                    Uri = uri,
                    Diagnostics =
                    [
                        new()
                        {
                            Message = result.Message,
                            Source = "CheckTp",
                            Severity = DiagnosticSeverity.Error,
                            Range = new()
                            {
                                Start = new(){ Line = result.Remainder.Line - 1, Character = result.Remainder.Column - 1 },
                                End = new(){ Line = result.Remainder.Line - 1, Character = result.Remainder.Column - 1 },
                            }
                        }
                    ]
                }
            },
            { WasSuccessful: true } => null,
        };

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
