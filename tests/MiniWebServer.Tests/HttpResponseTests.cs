using System.Text;
using MiniWebServer.Http;

namespace MiniWebServer.Tests;

public class HttpResponseTests
{
    [Fact]
    public void ToBytes_FormatedCorrectly()
    {
        var response = HttpResponse.Text("Hello", 200);
        byte[] bytes = response.ToBytes();
        string text = Encoding.UTF8.GetString(bytes);

        Assert.StartsWith("HTTP/1.1 200 OK\r\n", text);
        Assert.Contains("Content-Type: text/plain; charset=utf-8\r\n", text);
        Assert.Contains("Content-Length: 5\r\n", text);
        Assert.EndsWith("\r\n\r\nHello", text);
    }

    [Fact]
    public void JsonResponse_SetsCorrectHeaders()
    {
        var response = HttpResponse.Json("{\"status\":\"ok\"}");
        Assert.Equal("application/json", response.Headers.Get("Content-Type"));
        Assert.Equal(200, response.StatusCode);
    }
}
