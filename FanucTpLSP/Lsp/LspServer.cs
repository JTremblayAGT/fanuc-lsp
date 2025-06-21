using FanucTpLsp.JsonRPC;
using FanucTpLsp.Lsp.State;
using KarelParser;
using Sprache;
using TPLangParser.TPLang;

namespace FanucTpLsp.Lsp;

public class LspServer(string logFilePath)
{
    private readonly LspServerState _state = new(logFilePath);

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
            LspMethods.TextDocumentFormatting => HandleTextDocumentFormatting(json),
            LspMethods.TextDocumentRangeFormatting => HandleTextDocumentRangeFormatting(json),

            LspMethods.Shutdown => HandleShutdownRequest(),
            _ => null
        };

    private bool Initialize() => _state.Initialize();

    private InitializeResponse? HandleInitializeRequest(string json)
    {
        var request = JsonRpcDecoder.Decode<InitializeRequest>(json);
        if (request == null)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode InitializeRequest");
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
                    HoverProvider = true,
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
                    FormattingProvider = true,
                    RangeFormattingProvider = true,
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
        if (JsonRpcDecoder.Decode<TextDocumentDidOpenNotification>(json)
                is not TextDocumentDidOpenNotification notification)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentDidOpenNotification");
        }
        LogMessage($"[TextDocumentDidOpen]: {notification.Params.TextDocument.Uri}");

        if (_state.OpenedTextDocuments.ContainsKey(notification.Params.TextDocument.Uri))
        {
            LogMessage($"[TextDocumentDidOpen]: Document already opened: {notification?.Params.TextDocument.Uri}");
            return null;
        }

        if (notification.Params.TextDocument.Uri.EndsWith(".kl", StringComparison.OrdinalIgnoreCase))
        {
            var diagnostics = _state.OnKarelDocumentOpen(notification.Params.TextDocument);
            // TODO: build results
            return null;
        }

        var result = _state.OnTpDocumentOpen(notification.Params.TextDocument);

        return ParseResultToDiagnostics(result, notification.Params.TextDocument.Uri);
    }

    private ResponseMessage? HandleTextDocumentDidClose(string json)
    {
        if (JsonRpcDecoder.Decode<TextDocumentDidCloseNotification>(json)
                is not TextDocumentDidCloseNotification notification)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentDidCloseNotification");
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
        if (JsonRpcDecoder.Decode<TextDocumentDidChangeNotification>(json)
                is not TextDocumentDidChangeNotification notification)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentDidChangeParams");
        }

        var changedDocumentUri = notification.Params.TextDocument.Uri;
        LogMessage($"[TextDocumentDidChange]: {changedDocumentUri}");

        _state.UpdateDocumentText(changedDocumentUri, notification.Params.ContentChanges);

        return null;
    }

    private PublishDiagnosticsNotification? HandleTextDocumentDidSave(string json)
    {
        if (JsonRpcDecoder.Decode<TextDocumentDidSaveNotification>(json)
                is not TextDocumentDidSaveNotification request)
        {
            throw new JsonRpcException(ErrorCodes.InvalidParams, "Failed to decode TextDocumentDidSaveNotification");
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
        if (JsonRpcDecoder.Decode<TextDocumentDidHoverRequest>(json)
                is not TextDocumentDidHoverRequest request)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentDidHoverNotification");
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
        if (JsonRpcDecoder.Decode<TextDocumentDefinitionRequest>(json)
                is not TextDocumentDefinitionRequest request)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentDefinitionRequest");
        }
        LogMessage($"[TextDocumentDefinition]: {request.Params.TextDocument.Uri}");

        return new()
        {
            Id = request.Id,
            Result = _state.GetLocation(request.Params.TextDocument.Uri, request.Params.Position),
        };
    }

    private TextDocumentCodeActionResponse? HandleTextDocumentCodeAction(string json)
    {
        if (JsonRpcDecoder.Decode<TextDocumentCodeActionRequest>(json)
                is not TextDocumentCodeActionRequest request)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentCodeActionRequest");
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
        if (JsonRpcDecoder.Decode<TextDocumentCompletionRequest>(json)
                is not TextDocumentCompletionRequest request)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentCompletionRequest");
        }

        return new()
        {
            Id = request.Id,
            Result = _state.GetCompletionItems()
        };
    }

    private TextDocumentFormattingResponse? HandleTextDocumentFormatting(string json)
    {
        if (JsonRpcDecoder.Decode<TextDocumentFormattingRequest>(json)
                is not TextDocumentFormattingRequest request)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentFormattingResponse");
        }
        return null;
    }

    private TextDocumentFormattingResponse? HandleTextDocumentRangeFormatting(string json)
    {
        if (JsonRpcDecoder.Decode<TextDocumentRangeFormattingRequest>(json)
                is not TextDocumentRangeFormattingRequest request)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentRangeFormattingResponse");
        }
        return null;
    }

    private ResponseMessage? HandleShutdownRequest()
    {
        // Handle the shutdown request
        LogMessage("Server is shutting down.");
        Environment.Exit(0);

        // Unreachable code, but required for the method signature
        return null;
    }

    private static PublishDiagnosticsNotification? ParseResultToDiagnostics(
            IResult<TpProgram> result,
            string uri)
        => new()
        {
            Params = new()
            {
                Uri = uri,
                Diagnostics = result switch
                {
                    { WasSuccessful: false } =>
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
                    ],
                    _ => []
                }
            }
        };

    private static PublishDiagnosticsNotification? ParseKarelResultToDiagnostics(
            IResult<KarelProgram> result,
            string uri)
        => new()
        {
            Params = new()
            {
                Uri = uri,
                Diagnostics = result switch
                {
                    { WasSuccessful: false } =>
                    [
                        new()
                        {
                            Message = result.Message,
                            Source = "KarelParse",
                            Severity = DiagnosticSeverity.Error,
                            Range = new()
                            {
                                Start = new(){ Line = result.Remainder.Line - 1, Character = result.Remainder.Column - 1 },
                                End = new(){ Line = result.Remainder.Line - 1, Character = result.Remainder.Column - 1 },
                            }
                        }
                    ],
                    _ => []
                }
            }
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
