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

    [Fact]
    public void HeadResponse_OmitsBodyButKeepsContentLength()
    {
        var response = HttpResponse.Text("Hello", 200);
        byte[] bytes = response.ToBytes(omitBody: true);
        string text = Encoding.UTF8.GetString(bytes);

        Assert.Contains("Content-Length: 5\r\n", text);
        Assert.EndsWith("\r\n\r\n", text);
        Assert.Equal(Encoding.UTF8.GetBytes(text).Length, bytes.Length);
    }

    [Fact]
    public void Redirect_SetsLocationHeaderAndStatus()
    {
        var response = HttpResponse.Redirect("/home", 301);

        Assert.Equal(301, response.StatusCode);
        Assert.Equal("/home", response.Headers.Get("Location"));
        Assert.Contains("HTTP/1.1 301 Moved Permanently", Encoding.UTF8.GetString(response.ToBytes()));
    }

    [Fact]
    public void EmptyResponse_Uses204NoContent()
    {
        var response = HttpResponse.Empty();

        Assert.Equal(204, response.StatusCode);
        Assert.Empty(response.Body);
    }
}
