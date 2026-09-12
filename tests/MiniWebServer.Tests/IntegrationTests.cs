using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
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

    [Fact]
    public async Task Server_KeepAlive_ServesMultipleRequestsOnOneConnection()
    {
        int port = 9105;
        var server = new WebServer(IPAddress.Loopback, port);
        server.Router.Get("/one", (req) => Task.FromResult(HttpResponse.Text("first")));
        server.Router.Get("/two", (req) => Task.FromResult(HttpResponse.Text("second")));
        server.Start();

        try
        {
            using var client = new TcpClient();
            client.Connect(IPAddress.Loopback, port);
            using NetworkStream stream = client.GetStream();
            byte[] pipelined = Encoding.UTF8.GetBytes(
                "GET /one HTTP/1.1\r\nHost: localhost\r\n\r\n" +
                "GET /two HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n");

            await stream.WriteAsync(pipelined);
            string first = await ReadResponseAsync(stream);
            string second = await ReadResponseAsync(stream);

            Assert.Contains("HTTP/1.1 200 OK", first);
            Assert.EndsWith("\r\n\r\nfirst", first);
            Assert.Contains("Connection: keep-alive", first);
            Assert.Contains("HTTP/1.1 200 OK", second);
            Assert.EndsWith("\r\n\r\nsecond", second);
            Assert.Contains("Connection: close", second);
        }
        finally
        {
            server.Stop();
        }
    }

    [Fact]
    public async Task Server_WrongMethod_Returns405WithAllowHeader()
    {
        int port = 9108;
        var server = new WebServer(IPAddress.Loopback, port);
        server.Router.Get("/resource", (req) => Task.FromResult(HttpResponse.Text("ok")));
        server.Start();

        try
        {
            using var client = new TcpClient();
            client.Connect(IPAddress.Loopback, port);
            using NetworkStream stream = client.GetStream();
            byte[] request = Encoding.UTF8.GetBytes("POST /resource HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(request);

            string response = await ReadResponseAsync(stream);

            Assert.Contains("HTTP/1.1 405 Method Not Allowed", response);
            Assert.Contains("Allow: GET, HEAD", response);
        }
        finally
        {
            server.Stop();
        }
    }

    [Fact]
    public async Task Server_Head_ReturnsHeadersWithoutBody()
    {
        int port = 9109;
        var server = new WebServer(IPAddress.Loopback, port);
        server.Router.Get("/ping", (req) => Task.FromResult(HttpResponse.Text("pong")));
        server.Start();

        try
        {
            using var client = new HttpClient();
            using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"http://127.0.0.1:{port}/ping"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(4, response.Content.Headers.ContentLength);
            Assert.Equal("", await response.Content.ReadAsStringAsync());
        }
        finally
        {
            server.Stop();
        }
    }

    [Fact]
    public async Task Server_FormPost_ParsesFormAndResponds()
    {
        int port = 9111;
        var server = new WebServer(IPAddress.Loopback, port);
        server.Router.Post("/form", (req) => Task.FromResult(HttpResponse.Text($"Hello, {req.Form["name"]}!")));
        server.Start();

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.ConnectionClose = true;
            using var response = await client.PostAsync(
                $"http://127.0.0.1:{port}/form",
                new FormUrlEncodedContent(new Dictionary<string, string> { ["name"] = "Kapil" }));

            Assert.Equal("Hello, Kapil!", await response.Content.ReadAsStringAsync());
        }
        finally
        {
            server.Stop();
        }
    }

    [Fact]
    public async Task Server_Redirect_ReturnsLocationHeader()
    {
        int port = 9112;
        var server = new WebServer(IPAddress.Loopback, port);
        server.Router.Get("/old-path", (req) => Task.FromResult(HttpResponse.Redirect("/new-path")));
        server.Start();

        try
        {
            using var handler = new HttpClientHandler { AllowAutoRedirect = false };
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.ConnectionClose = true;
            using var response = await client.GetAsync($"http://127.0.0.1:{port}/old-path");

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/new-path", response.Headers.Location?.ToString());
        }
        finally
        {
            server.Stop();
        }
    }

    private static async Task<string> ReadResponseAsync(NetworkStream stream)
    {
        var headerBuilder = new StringBuilder();
        int b;
        while ((b = stream.ReadByte()) != -1)
        {
            headerBuilder.Append((char)b);
            if (headerBuilder.Length >= 4 && headerBuilder.ToString(headerBuilder.Length - 4, 4) == "\r\n\r\n")
            {
                break;
            }
        }

        string headerText = headerBuilder.ToString();
        int contentLength = 0;
        foreach (string line in headerText.Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                int.TryParse(line["Content-Length:".Length..].Trim(), out contentLength);
            }
        }

        byte[] body = new byte[contentLength];
        int totalRead = 0;
        while (totalRead < contentLength)
        {
            int read = await stream.ReadAsync(body.AsMemory(totalRead, contentLength - totalRead));
            if (read == 0) break;
            totalRead += read;
        }

        return headerText + Encoding.UTF8.GetString(body, 0, totalRead);
    }
}
