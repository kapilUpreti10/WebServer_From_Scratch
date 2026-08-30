using System.Net;
using System.Net.Http;
using MiniWebServer.Http;
using MiniWebServer.Server;

namespace MiniWebServer.Tests;

public class IntegrationTests
{
    [Fact]
    public async Task Server_HandlesEndToEndRequest()
    {
        int port = 9099;
        var server = new WebServer(IPAddress.Loopback, port);
        server.Router.Get("/ping", (req) => Task.FromResult(HttpResponse.Text("pong")));
        server.Start();

        try
        {
            using var client = new HttpClient();
            string result = await client.GetStringAsync($"http://127.0.0.1:{port}/ping");
            Assert.Equal("pong", result);
        }
        finally
        {
            server.Stop();
        }
    }
}
