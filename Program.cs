using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Protocol;

var stdin = Console.OpenStandardInput();
var stdout = Console.OpenStandardOutput();
var reader = new StreamReader(stdin, Encoding.UTF8);
var writer = new StreamWriter(stdout, Encoding.UTF8) { AutoFlush = true };

while (true)
{
    // Read headers (Content-Length)
    string? line;
    int contentLength = 0;
    while (!string.IsNullOrEmpty(line = reader.ReadLine()))
    {
        if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
        {
            var parts = line.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int len))
                contentLength = len;
        }
    }

    if (contentLength == 0)
        continue;

    // Read content
    char[] buffer = new char[contentLength];
    int read = 0;
    while (read < contentLength)
    {
        int n = reader.Read(buffer, read, contentLength - read);
        if (n == 0) break;
        read += n;
    }
    var json = new string(buffer, 0, read);

    // Deserialize and handle message
    var doc = JsonDocument.Parse(json);
    if (doc.RootElement.TryGetProperty("method", out _))
    {
        // Notification or request
        if (doc.RootElement.TryGetProperty("id", out _))
        {
            // Request: echo back a dummy response
            var request = JsonSerializer.Deserialize<RequestMessage>(json);
            var response = new ResponseMessage
            {
                Id = request?.Id,
                Result = new { message = "ok" }
            };
            WriteResponse(writer, response);
        }
        // else: Notification, do nothing
    }
    // else: Response, ignore for server
}

static void WriteResponse(StreamWriter writer, ResponseMessage response)
{
    var json = JsonSerializer.Serialize(response);
    var bytes = Encoding.UTF8.GetBytes(json);
    writer.Write($"Content-Length: {bytes.Length}\r\n\r\n");
    writer.Write(json);
    writer.Flush();
}
