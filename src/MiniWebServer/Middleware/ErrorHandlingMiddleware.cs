using MiniWebServer.Http;

namespace MiniWebServer.Middleware;

public class ErrorHandlingMiddleware : IMiddleware
{
    public async Task<HttpResponse> InvokeAsync(HttpRequest request, RequestDelegate next)
    {
        try
        {
            return await next(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Unhandled exception: {ex.Message}");
            return HttpResponse.Text($"Internal Server Error: {ex.Message}", 500);
        }
    }
}
