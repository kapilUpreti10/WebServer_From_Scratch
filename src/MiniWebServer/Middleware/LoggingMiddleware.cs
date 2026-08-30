using MiniWebServer.Http;

namespace MiniWebServer.Middleware;

public class LoggingMiddleware : IMiddleware
{
    public async Task<HttpResponse> InvokeAsync(HttpRequest request, RequestDelegate next)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] IN  {request.Method} {request.RawUrl}");
        var response = await next(request);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] OUT {request.Method} {request.RawUrl} -> {response.StatusCode}");
        return response;
    }
}
