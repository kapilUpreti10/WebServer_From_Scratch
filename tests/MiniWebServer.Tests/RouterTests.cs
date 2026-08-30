using MiniWebServer.Http;
using MiniWebServer.Routing;

namespace MiniWebServer.Tests;

public class RouterTests
{
    [Fact]
    public void Match_ExactRoute_ReturnsHandler()
    {
        var router = new Router();
        router.Get("/hello", (req) => Task.FromResult(HttpResponse.Text("hello")));

        var req = new HttpRequest { Method = "GET", Path = "/hello" };
        var handler = router.Match(req);

        Assert.NotNull(handler);
    }

    [Fact]
    public void Match_ParameterizedRoute_ExtractsParams()
    {
        var router = new Router();
        router.Get("/users/{id}", (req) => Task.FromResult(HttpResponse.Text($"User {req.RouteParams["id"]}")));

        var req = new HttpRequest { Method = "GET", Path = "/users/42" };
        var handler = router.Match(req);

        Assert.NotNull(handler);
        Assert.Equal("42", req.RouteParams["id"]);
    }

    [Fact]
    public void Match_NotFound_ReturnsNull()
    {
        var router = new Router();
        router.Get("/hello", (req) => Task.FromResult(HttpResponse.Text("hello")));

        var req = new HttpRequest { Method = "GET", Path = "/nonexistent" };
        var handler = router.Match(req);

        Assert.Null(handler);
    }
}
