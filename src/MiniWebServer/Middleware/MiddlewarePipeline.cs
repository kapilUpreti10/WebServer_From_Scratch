using MiniWebServer.Http;

namespace MiniWebServer.Middleware;

public class MiddlewarePipeline
{
    private readonly List<IMiddleware> _middlewares = new();

    public void Use(IMiddleware middleware)
    {
        _middlewares.Add(middleware);
    }

    public RequestDelegate Build(RequestDelegate fallbackHandler)
    {
        RequestDelegate current = fallbackHandler;
        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var next = current;
            current = (request) => middleware.InvokeAsync(request, next);
        }
        return current;
    }
}
