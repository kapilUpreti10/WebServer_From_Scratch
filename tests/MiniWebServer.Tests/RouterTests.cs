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

    [Fact]
    public void Match_PatchRoute_ReturnsHandler()
    {
        var router = new Router();
        router.Patch("/items/{id}", (req) => Task.FromResult(HttpResponse.Text($"Patching {req.RouteParams["id"]}")));

        var req = new HttpRequest { Method = "PATCH", Path = "/items/7" };
        var handler = router.Match(req);

        Assert.NotNull(handler);
        Assert.Equal("7", req.RouteParams["id"]);
    }

    [Fact]
    public void Match_HeadRequest_FallsBackToGetHandler()
    {
        var router = new Router();
        router.Get("/ping", (req) => Task.FromResult(HttpResponse.Text("pong")));

        var req = new HttpRequest { Method = "HEAD", Path = "/ping" };
        var handler = router.Match(req);

        Assert.NotNull(handler);
    }

    [Fact]
    public void GetAllowedMethods_PathWithGet_ReturnsHeadAndGet()
    {
        var router = new Router();
        router.Get("/resource", (req) => Task.FromResult(HttpResponse.Text("ok")));

        string[]? allowed = router.GetAllowedMethods("/resource");

        Assert.NotNull(allowed);
        Assert.Contains("GET", allowed);
        Assert.Contains("HEAD", allowed);
    }

    [Fact]
    public void GetAllowedMethods_UnknownPath_ReturnsNull()
    {
        var router = new Router();
        router.Get("/resource", (req) => Task.FromResult(HttpResponse.Text("ok")));

        Assert.Null(router.GetAllowedMethods("/missing"));
    }
}
