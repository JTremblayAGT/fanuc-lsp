using System.Text;
using System.Text.Json;
using System.IO;
using JsonRPC;

// Create log directory and file path
var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
Directory.CreateDirectory(logDirectory);
var logFilePath = Path.Combine(logDirectory, $"server_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

var stdin = Console.OpenStandardInput();
var stdout = Console.OpenStandardOutput();
var reader = new StreamReader(stdin, Encoding.UTF8);
var writer = new StreamWriter(stdout, Encoding.UTF8) { AutoFlush = true };

// Log server start
LogMessage(logFilePath, $"Server started at {DateTime.Now}");

while (true)
{
    // Read headers (Content-Length)
    string? line;
    var contentLength = 0;
    while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
    {
        if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
        {
            var parts = line.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out var len))
            {
                contentLength = len;
            }
        }
    }

    if (contentLength == 0)
    {
        continue;
    }

    // Read content
    var buffer = new char[contentLength];
    var read = 0;
    while (read < contentLength)
    {
        var n = reader.Read(buffer, read, contentLength - read);
        if (n == 0)
        {
            break;
        }
        read += n;
    }
    var json = new string(buffer, 0, read);

    // Log the received message
    LogMessage(logFilePath, $"RECEIVED: {json}");

    // Deserialize and handle message
    var doc = JsonDocument.Parse(json);
    if (doc.RootElement.TryGetProperty("method", out _))
    {
        // Notification or request
        if (doc.RootElement.TryGetProperty("id", out _))
        {
            // Request: echo back a dummy response
            var request = JsonRpcDecoder.Decode<RequestMessage>(json);
            var response = new ResponseMessage
            {
                Id = request?.Id,
                Result = new { message = "ok" }
            };
            WriteResponse(writer, response, logFilePath);
        }
        // else: Notification, do nothing
    }
    // else: Response, ignore for server
}

static void WriteResponse(StreamWriter writer, ResponseMessage response, string logFilePath)
{
    var json = JsonRpcEncoder.Encode(response);
    var bytes = Encoding.UTF8.GetBytes(json);
    writer.Write($"Content-Length: {bytes.Length}\r\n\r\n");
    writer.Write(json);
    writer.Flush();

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
