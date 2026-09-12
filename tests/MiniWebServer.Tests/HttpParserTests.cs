using System.Text;
using MiniWebServer.Http;

namespace MiniWebServer.Tests;

public class HttpParserTests
{
    [Fact]
    public async Task ParseAsync_ValidGetRequest_ParsesCorrectly()
    {
        string raw = "GET /test?name=kapil HTTP/1.1\r\nHost: localhost\r\nUser-Agent: TestAgent\r\n\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(raw));

        var request = await HttpParser.ParseAsync(stream);

        Assert.NotNull(request);
        Assert.Equal("GET", request.Method);
        Assert.Equal("/test", request.Path);
        Assert.Equal("/test?name=kapil", request.RawUrl);
        Assert.Equal("HTTP/1.1", request.HttpVersion);
        Assert.Equal("localhost", request.Headers.Get("Host"));
        Assert.Equal("kapil", request.Query["name"]);
    }

    [Fact]
    public async Task ParseAsync_PostRequestWithBody_ParsesBody()
    {
        string raw = "POST /api/data HTTP/1.1\r\nContent-Length: 11\r\n\r\nHello World";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(raw));

        var request = await HttpParser.ParseAsync(stream);

        Assert.NotNull(request);
        Assert.Equal("POST", request.Method);
        Assert.Equal("Hello World", request.BodyText);
    }

    [Fact]
    public async Task ParseAsync_UrlEncodedBody_ParsesForm()
    {
        string raw = "POST /form HTTP/1.1\r\nContent-Type: application/x-www-form-urlencoded\r\nContent-Length: 30\r\n\r\nname=Kapil+Mishra&role=student";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(raw));

        var request = await HttpParser.ParseAsync(stream);

        Assert.NotNull(request);
        Assert.Equal("Kapil Mishra", request.Form["name"]);
        Assert.Equal("student", request.Form["role"]);
    }
}
