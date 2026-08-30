using System.Text;

namespace MiniWebServer.Http;

public class HttpResponse
{
    public int StatusCode { get; set; } = 200;
    public string StatusReason { get; set; } = "OK";
    public HttpHeaders Headers { get; } = new();
    public byte[] Body { get; set; } = Array.Empty<byte>();

    public HttpResponse()
    {
        Headers["Server"] = "MiniWebServer/1.0";
    }

    public static HttpResponse Html(string html, int statusCode = 200) => new()
    {
        StatusCode = statusCode,
        StatusReason = GetDefaultReason(statusCode),
        Headers = { ["Content-Type"] = "text/html; charset=utf-8" },
        Body = Encoding.UTF8.GetBytes(html)
    };

    public static HttpResponse Text(string text, int statusCode = 200) => new()
    {
        StatusCode = statusCode,
        StatusReason = GetDefaultReason(statusCode),
        Headers = { ["Content-Type"] = "text/plain; charset=utf-8" },
        Body = Encoding.UTF8.GetBytes(text)
    };

    public static HttpResponse Json(string json, int statusCode = 200) => new()
    {
        StatusCode = statusCode,
        StatusReason = GetDefaultReason(statusCode),
        Headers = { ["Content-Type"] = "application/json" },
        Body = Encoding.UTF8.GetBytes(json)
    };

    public byte[] ToBytes()
    {
        Headers["Content-Length"] = Body.Length.ToString();
        var sb = new StringBuilder();
        sb.Append($"HTTP/1.1 {StatusCode} {StatusReason}\r\n");
        foreach (var header in Headers)
        {
            sb.Append($"{header.Key}: {header.Value}\r\n");
        }
        sb.Append("\r\n");

        byte[] headerBytes = Encoding.UTF8.GetBytes(sb.ToString());
        byte[] fullBytes = new byte[headerBytes.Length + Body.Length];
        Buffer.BlockCopy(headerBytes, 0, fullBytes, 0, headerBytes.Length);
        Buffer.BlockCopy(Body, 0, fullBytes, headerBytes.Length, Body.Length);
        return fullBytes;
    }

    public static string GetDefaultReason(int statusCode) => statusCode switch
    {
        200 => "OK",
        201 => "Created",
        204 => "No Content",
        400 => "Bad Request",
        404 => "Not Found",
        500 => "Internal Server Error",
        _ => "OK"
    };
}
