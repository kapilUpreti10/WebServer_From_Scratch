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

    public static HttpResponse Redirect(string location, int statusCode = 302) => new()
    {
        StatusCode = statusCode,
        StatusReason = GetDefaultReason(statusCode),
        Headers =
        {
            ["Content-Type"] = "text/plain; charset=utf-8",
            ["Location"] = location
        },
        Body = Encoding.UTF8.GetBytes("Redirecting to " + location)
    };

    public static HttpResponse Empty(int statusCode = 204) => new()
    {
        StatusCode = statusCode,
        StatusReason = GetDefaultReason(statusCode)
    };

    public byte[] ToBytes(bool omitBody = false)
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
        int bodyLength = omitBody ? 0 : Body.Length;
        byte[] fullBytes = new byte[headerBytes.Length + bodyLength];
        Buffer.BlockCopy(headerBytes, 0, fullBytes, 0, headerBytes.Length);
        if (!omitBody)
        {
            Buffer.BlockCopy(Body, 0, fullBytes, headerBytes.Length, Body.Length);
        }
        return fullBytes;
    }

    public static string GetDefaultReason(int statusCode) => statusCode switch
    {
        200 => "OK",
        201 => "Created",
        202 => "Accepted",
        204 => "No Content",
        301 => "Moved Permanently",
        302 => "Found",
        303 => "See Other",
        304 => "Not Modified",
        307 => "Temporary Redirect",
        308 => "Permanent Redirect",
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        405 => "Method Not Allowed",
        408 => "Request Timeout",
        411 => "Length Required",
        413 => "Payload Too Large",
        415 => "Unsupported Media Type",
        422 => "Unprocessable Entity",
        429 => "Too Many Requests",
        500 => "Internal Server Error",
        501 => "Not Implemented",
        503 => "Service Unavailable",
        _ => "Unknown"
    };
}
