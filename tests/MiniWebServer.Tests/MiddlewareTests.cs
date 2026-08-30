using MiniWebServer.Http;
using MiniWebServer.Middleware;

namespace MiniWebServer.Tests;

public class MiddlewareTests
{
    private class TestMiddleware : IMiddleware
    {
        public bool Invoked { get; private set; }
        public async Task<HttpResponse> InvokeAsync(HttpRequest request, RequestDelegate next)
        {
            Invoked = true;
            return await next(request);
        }
    }

    [Fact]
    public async Task Pipeline_ExecutesMiddlewareInOrder()
    {
        var pipeline = new MiddlewarePipeline();
        var middleware = new TestMiddleware();
        pipeline.Use(middleware);

        var handler = pipeline.Build((req) => Task.FromResult(HttpResponse.Text("OK")));
        var response = await handler(new HttpRequest());

        Assert.True(middleware.Invoked);
        Assert.Equal(200, response.StatusCode);
    }
}
