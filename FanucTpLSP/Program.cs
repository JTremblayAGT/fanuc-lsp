using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FanucTpLsp.JsonRPC;
using FanucTpLsp.Lsp;

using Sprache;

// Create log directory and file path
var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
Directory.CreateDirectory(logDirectory);
var logFilePath = Path.Combine(logDirectory, $"server_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

var stdin = Console.OpenStandardInput();
var stdout = Console.OpenStandardOutput();
var reader = new StreamReader(stdin, Encoding.UTF8);
var writer = new StreamWriter(stdout, Encoding.UTF8);

// Log server start
LogMessage(logFilePath, $"Server started at {DateTime.Now}");

// TODO: Make the server connect to the robot defined in some config file in project root
var server = new LspServer(logFilePath);

//while (!Debugger.IsAttached)
//{
//    Thread.Sleep(1000);
//}

while (true)
{
    try
    {
        // Read and decode request using JsonRpcDecoder
        StringBuilder rawRequest = new();
        var header = reader.ReadLine();
        while (!string.IsNullOrEmpty(header))
        {
            rawRequest.AppendLine(header);
            header = reader.ReadLine();
        }

        // Read the rest of the content (if any)
        if (rawRequest.Length == 0)
        {
            LogMessage(logFilePath, "Raw Request is empty.");
            continue;
        }

        // Peek at Content-Length header to determine how much to read
        var headers = rawRequest.ToString();
        var contentLength = 0;
        try
        {
            contentLength = JsonRpcDecoder.HeaderParser.Parse(headers);
        }
        catch
        {
            LogMessage(logFilePath, "Failed to decode the content length.");
            continue;
        }

        var buffer = new char[contentLength];
        var read = 0;
        while (read < contentLength)
        {
            int n = reader.Read(buffer, read, contentLength - read);
            if (n == 0)
            {
                break;
            }

            read += n;
        }

        var json = new string(buffer, 0, read);

        await HandleRequest(server, json, writer, logFilePath);
    }
    catch (Exception ex)
    {
        LogMessage(logFilePath, $"Unhandled Exception: {ex.Message}");
    }
}

static async Task HandleRequest(LspServer server, string json, StreamWriter writer, string logFilePath)
{
    var doc = JsonDocument.Parse(json);
    if (!doc.RootElement.TryGetProperty("method", out var method))
    {
        // TODO: some error handling
        await WriteResponse(writer, new ResponseMessage(), logFilePath);
    }

    LogMessage(logFilePath, $"RECEIVED: [{method}]");
    try
    {
        var response = server.HandleRequest(method.GetString()!, json);
        if (response == null)
        {
            return;
        }
        await WriteResponse(writer, response, logFilePath);
    }
    catch (JsonRpcException ex)
    {
        LogMessage(logFilePath, $"An error occured while handling the request: {ex.Message}");
        // TODO: implement error handling
        var errorResponse = new ResponseMessage
        {
            /*
            Error = new ResponseError
            {
                Code = ex.Code,
                Message = ex.Message,
                Data = ex.Data
            }
            */
        };
        await WriteResponse(writer, errorResponse, logFilePath);
    }
}

static async Task WriteResponse(StreamWriter writer, ResponseMessage response, string logFilePath)
{
    var json = JsonRpcEncoder.Encode(response);
    var bytes = json.ToCharArray();
    await writer.WriteAsync(bytes, 0, bytes.Length);
    await writer.FlushAsync();

    // Log the sent response
    LogMessage(logFilePath, $"SENT: {json}");
}

static void LogMessage(string logFilePath, string message)
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
