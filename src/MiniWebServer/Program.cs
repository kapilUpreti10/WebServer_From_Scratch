using System.Net;
using MiniWebServer.Http;
using MiniWebServer.Middleware;
using MiniWebServer.Server;
using MiniWebServer.StaticFiles;

var server = new WebServer(IPAddress.Loopback, 8080);

// Add Middlewares
server.Pipeline.Use(new ErrorHandlingMiddleware());
server.Pipeline.Use(new LoggingMiddleware());

// Add Static Files support (from a wwwroot folder)
string wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwroot))
{
    Directory.CreateDirectory(wwwroot);
    File.WriteAllText(Path.Combine(wwwroot, "index.html"), "<h1>Welcome to MiniWebServer!</h1>");
}
server.Pipeline.Use(new StaticFileMiddleware(wwwroot));

// Add Routes
server.Router.Get("/", (req) => Task.FromResult(HttpResponse.Html("<h1>Home Page</h1>")));
server.Router.Get("/hello", (req) => Task.FromResult(HttpResponse.Text("Hello, World!")));
server.Router.Get("/users/{id}", (req) =>
{
    string userId = req.RouteParams["id"];
    return Task.FromResult(HttpResponse.Json($"{{\"userId\": \"{userId}\"}}"));
});
server.Router.Post("/echo", (req) => Task.FromResult(HttpResponse.Text($"Echo: {req.BodyText}")));

// Form body parsing (application/x-www-form-urlencoded)
server.Router.Post("/form", (req) =>
    Task.FromResult(HttpResponse.Text($"Hello, {req.Form.GetValueOrDefault("name", "stranger")}!")));

// Redirect helper
server.Router.Get("/redirect", (req) => Task.FromResult(HttpResponse.Redirect("/")));

// 405: the path exists (registered for POST below) but not for GET
server.Router.Post("/only-post", (req) => Task.FromResult(HttpResponse.Text("POST works")));

server.Start();

Console.WriteLine("Press ENTER to stop the server...");
Console.ReadLine();

server.Stop();
