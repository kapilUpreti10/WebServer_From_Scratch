using MiniWebServer.Http;

namespace MiniWebServer.StaticFiles;

public class StaticFileMiddleware : Middleware.IMiddleware
{
    private readonly string _rootPath;

    public StaticFileMiddleware(string rootPath)
    {
        _rootPath = Path.GetFullPath(rootPath);
    }

    public async Task<HttpResponse> InvokeAsync(HttpRequest request, Middleware.RequestDelegate next)
    {
        if (request.Method != "GET" && request.Method != "HEAD")
        {
            return await next(request);
        }

        string relativePath = request.Path.TrimStart('/');
        if (string.IsNullOrEmpty(relativePath))
        {
            relativePath = "index.html";
        }

        string fullPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));

        // Path Traversal Security Check
        if (!fullPath.StartsWith(_rootPath, StringComparison.Ordinal))
        {
            return HttpResponse.Text("403 Forbidden: Invalid Path", 403);
        }

        if (File.Exists(fullPath))
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(fullPath);
            string contentType = GetContentType(fullPath);
            return new HttpResponse
            {
                StatusCode = 200,
                StatusReason = "OK",
                Headers = { ["Content-Type"] = contentType },
                Body = fileBytes
            };
        }

        return await next(request);
    }

    private static string GetContentType(string filePath) => Path.GetExtension(filePath).ToLowerInvariant() switch
    {
        ".html" or ".htm" => "text/html; charset=utf-8",
        ".css" => "text/css",
        ".js" or ".mjs" => "application/javascript",
        ".json" => "application/json",
        ".txt" => "text/plain; charset=utf-8",
        ".xml" => "application/xml",
        ".csv" => "text/csv",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".svg" => "image/svg+xml",
        ".webp" => "image/webp",
        ".avif" => "image/avif",
        ".ico" => "image/x-icon",
        ".bmp" => "image/bmp",
        ".pdf" => "application/pdf",
        ".zip" => "application/zip",
        ".woff" => "font/woff",
        ".woff2" => "font/woff2",
        ".ttf" => "font/ttf",
        ".otf" => "font/otf",
        ".mp4" => "video/mp4",
        ".webm" => "video/webm",
        ".mp3" => "audio/mpeg",
        ".wav" => "audio/wav",
        ".wasm" => "application/wasm",
        _ => "application/octet-stream"
    };
}
