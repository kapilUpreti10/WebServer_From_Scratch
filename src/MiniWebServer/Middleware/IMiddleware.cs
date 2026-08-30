using MiniWebServer.Http;

namespace MiniWebServer.Middleware;

public delegate Task<HttpResponse> RequestDelegate(HttpRequest request);

public interface IMiddleware
{
    Task<HttpResponse> InvokeAsync(HttpRequest request, RequestDelegate next);
}
