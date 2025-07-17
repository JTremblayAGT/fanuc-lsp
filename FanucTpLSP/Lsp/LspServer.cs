using FanucTpLsp.JsonRPC;
using FanucTpLsp.Lsp.Format;
using FanucTpLsp.Lsp.State;
using KarelParser;
using Sprache;
using TPLangParser.TPLang;

namespace FanucTpLsp.Lsp;

public class LspServer(string logFilePath)
{
    private readonly LspServerState _state = new(logFilePath);

    public async Task HandleRequest(string method, string json, Func<ResponseMessage, Task> callback)
    {
        switch (method)
        {
            case LspMethods.Initialize:
                await HandleInitializeRequest(json, callback);
                break;
            case LspMethods.Initialized:
                await HandleInitializedNotification(json, callback);
                break;
            case LspMethods.TextDocumentDidOpen:
                await HandleTextDocumentDidOpen(json, callback);
                break;
            case LspMethods.TextDocumentDidClose:
                await HandleTextDocumentDidClose(json, callback);
                break;
            case LspMethods.TextDocumentDidChange:
                await HandleTextDocumentDidChange(json, callback);
                break;
            case LspMethods.TextDocumentDidSave:
                await HandleTextDocumentDidSave(json, callback);
                break;
            case LspMethods.TextDocumentDidHover:
                await HandleTextDocumentDidHover(json, callback);
                break;
            case LspMethods.TextDocumentDefinition:
                await HandleTextDocumentDefinition(json, callback);
                break;
            case LspMethods.TextDocumentCompletion:
                await HandleTextDocumentCompletion(json, callback);
                break;
            case LspMethods.TextDocumentFormatting:
                await HandleTextDocumentFormatting(json, callback);
                break;
            case LspMethods.TextDocumentCodeAction:
            case LspMethods.TextDocumentRangeFormatting:
                break;
            case LspMethods.Shutdown:
                HandleShutdownRequest();
                break;
            default:
                LogMessage($"Unsupported method {method}");
                //await callback(new ResponseError { Code = ErrorCodes.MethodNotFound });
                break;
        }
    }

    private bool Initialize() => _state.Initialize();

    private async Task HandleInitializeRequest(string json, Func<ResponseMessage, Task> callback)
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
                            "[", "]", "."
                        ]
                    },
                    FormattingProvider = true,
                    RangeFormattingProvider = false,
                },
                ServerInfo = new()
                {
                    Name = "fanuctp-lsp",
                    Version = "0.0.0-alpha1"
                }
            }
        };

        await callback(response);
    }

    private async Task HandleInitializedNotification(string json, Func<ResponseMessage, Task> callback)
    {
        var notification = JsonRpcDecoder.Decode<RequestMessage>(json);
        // Handle the initialized notification
        LogMessage($"NOTIFICATION: {notification?.Method}");

        await Task.Run(() => { /* TODO: notification */ }).ConfigureAwait(false);
    }

    private async Task HandleTextDocumentDidOpen(string json, Func<ResponseMessage, Task> callback)
    {
        if (JsonRpcDecoder.Decode<TextDocumentDidOpenNotification>(json)
                is not { } notification)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentDidOpenNotification");
        }
        LogMessage($"[TextDocumentDidOpen]: {notification.Params.TextDocument.Uri}");

        if (_state.OpenedTextDocuments.ContainsKey(notification.Params.TextDocument.Uri))
        {
            LogMessage($"[TextDocumentDidOpen]: Document already opened: {notification?.Params.TextDocument.Uri}");
            return;
        }

        if (notification.Params.TextDocument.Uri.EndsWith(".kl", StringComparison.OrdinalIgnoreCase))
        {
            await _state.OnKarelDocumentOpen(notification.Params.TextDocument)
                .ContinueWith(async result => ParseKarelResultToDiagnostics(await result, notification.Params.TextDocument.Uri))
                .ConfigureAwait(false);
            return;
        }

        await _state.OnTpDocumentOpen(notification.Params.TextDocument)
            .ContinueWith(async result => ParseResultToDiagnostics(await result, notification.Params.TextDocument.Uri))
            .ConfigureAwait(false);
    }

    private async Task HandleTextDocumentDidClose(string json, Func<ResponseMessage, Task> callback)
    {
        if (JsonRpcDecoder.Decode<TextDocumentDidCloseNotification>(json)
                is not { } notification)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentDidCloseNotification");
        }
        LogMessage($"[TextDocumentDidClose]: {notification.Params.TextDocument.Uri}");

        if (!_state.OpenedTextDocuments.ContainsKey(notification.Params.TextDocument.Uri))
        {
            LogMessage($"[TextDocumentDidClose]: Document not opened: {notification?.Params.TextDocument.Uri}");
        }

        await Task.Run(() => _state.OpenedTextDocuments.Remove(notification!.Params.TextDocument.Uri, out var _))
            .ConfigureAwait(false);
    }

    private async Task HandleTextDocumentDidChange(string json, Func<ResponseMessage, Task> callback)
    {
        if (JsonRpcDecoder.Decode<TextDocumentDidChangeNotification>(json)
                is not { } notification)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentDidChangeParams");
        }

        var changedDocumentUri = notification.Params.TextDocument.Uri;
        LogMessage($"[TextDocumentDidChange]: {changedDocumentUri}");

        await Task.Run(() => _state.UpdateDocumentText(changedDocumentUri, notification.Params.ContentChanges))
            .ConfigureAwait(false);
    }

    private async Task HandleTextDocumentDidSave(string json, Func<ResponseMessage, Task> callback)
    {
        if (JsonRpcDecoder.Decode<TextDocumentDidSaveNotification>(json)
                is not { } request)
        {
            throw new JsonRpcException(ErrorCodes.InvalidParams, "Failed to decode TextDocumentDidSaveNotification");
        }
        LogMessage($"[TextDocumentDidSave]: {request.Params.TextDocument.Uri}");
        if (!_state.OpenedTextDocuments.TryGetValue(request.Params.TextDocument.Uri, out var documentState))
        {
            LogMessage($"[TextDocumentDidChange]: Document not opened: {request.Params.TextDocument.Uri}");
            return;
        }

        switch (documentState.Type)
        {
            case DocumentType.Tp:
                await _state.UpdateParsedProgram(request.Params.TextDocument.Uri)
                    .ContinueWith(async result => ParseResultToDiagnostics(await result, request.Params.TextDocument.Uri))
                    .ConfigureAwait(false);
                break;
            case DocumentType.Karel:
                await _state.UpdateParsedKlProgram(request.Params.TextDocument.Uri)
                    .ContinueWith(async result => ParseKarelResultToDiagnostics(await result, request.Params.TextDocument.Uri))
                    .ConfigureAwait(false);
                break;
        }
        // Might not actually need to do anything
    }

    private async Task HandleTextDocumentDidHover(string json, Func<ResponseMessage, Task> callback)
    {
        if (JsonRpcDecoder.Decode<TextDocumentDidHoverRequest>(json)
                is not { } request)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentDidHoverNotification");
        }
        LogMessage($"[TextDocumentDidHover]: {request.Params.TextDocument.Uri}");

        await callback(new TextDocumentHoverResponse
        {
            Id = request.Id,
            Result = _state.GetHoverResult(request.Params.TextDocument.Uri, request.Params.Position),
        });
    }

    private async Task HandleTextDocumentDefinition(string json, Func<ResponseMessage, Task> callback)
    {
        if (JsonRpcDecoder.Decode<TextDocumentDefinitionRequest>(json)
                is not { } request)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentDefinitionRequest");
        }
        LogMessage($"[TextDocumentDefinition]: {request.Params.TextDocument.Uri}");

        await callback(new TextDocumentDefinitionResponse
        {
            Id = request.Id,
            Result = _state.GetLocation(request.Params.TextDocument.Uri, request.Params.Position),
        });
    }

    //private TextDocumentCodeActionResponse? HandleTextDocumentCodeAction(string json, Func<ResponseMessage, Task> callback)
    //{
    //    if (JsonRpcDecoder.Decode<TextDocumentCodeActionRequest>(json)
    //            is not { } request)
    //    {
    //        throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentCodeActionRequest");
    //    }
    //    LogMessage($"[TextDocumentCodeAction]: {request.Params.TextDocument.Uri}");

    //    if (!_state.OpenedTextDocuments.ContainsKey(request.Params.TextDocument.Uri))
    //    {
    //        LogMessage($"[TextDocumentCodeAction]: Document not opened: {request.Params.TextDocument.Uri}");
    //        return null;
    //    }

    //    // TODO: implement code actions (e.g. refactoring, line renumbering, syncing comments with robot, etc.)

    //    return new()
    //    {
    //        Id = request.Id,
    //        Result = []
    //    };
    //}

    private async Task HandleTextDocumentCompletion(string json, Func<ResponseMessage, Task> callback)
    {
        if (JsonRpcDecoder.Decode<TextDocumentCompletionRequest>(json)
                is not { } request)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentCompletionRequest");
        }

        await callback(new TextDocumentCompletionResponse
        {
            Id = request.Id,
            Result = _state.GetCompletionItems()
        });
    }

    private async Task HandleTextDocumentFormatting(string json, Func<ResponseMessage, Task> callback)
    {
        if (JsonRpcDecoder.Decode<TextDocumentFormattingRequest>(json)
                is not { } request)
        {
            throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentFormattingResponse");
        }

        if (!_state.OpenedTextDocuments.TryGetValue(request.Params.TextDocument.Uri, out var documentState))
        {
            var errMsg = $"[TextDocumentDidChange]: Document not opened: {request.Params.TextDocument.Uri}";
            LogMessage(errMsg);
            throw new JsonRpcException(ErrorCodes.InvalidRequest, errMsg);
        }

        await callback(new TextDocumentFormattingResponse
        {
            Id = request.Id,
            Result = [
                new()
                {
                    Range = ContentRange.WholeFile(documentState.TextDocument.Text),
                    NewText = documentState.Type switch
                    {
                        DocumentType.Tp => (new TpFormatter()).Format(documentState.TextDocument.Text, request.Params.Options),
                        DocumentType.Karel => documentState.TextDocument.Text, // TODO
                        _ => throw new JsonRpcException(ErrorCodes.InvalidRequest, $"Unsupported file type: [{documentState.Type}]")
                    }
                }
            ]
        });
    }

    //private TextDocumentFormattingResponse? HandleTextDocumentRangeFormatting(string json, Func<ResponseMessage, Task> callback)
    //{
    //    if (JsonRpcDecoder.Decode<TextDocumentRangeFormattingRequest>(json)
    //            is not { } request)
    //    {
    //        throw new JsonRpcException(ErrorCodes.InvalidRequest, "Failed to decode TextDocumentRangeFormattingResponse");
    //    }
    //    return null;
    //}

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
