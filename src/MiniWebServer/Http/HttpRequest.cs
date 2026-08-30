namespace MiniWebServer.Http;

public class HttpRequest
{
    public string Method { get; set; } = "GET";
    public string Path { get; set; } = "/";
    public string RawUrl { get; set; } = "/";
    public string HttpVersion { get; set; } = "HTTP/1.1";
    public HttpHeaders Headers { get; } = new();
    public Dictionary<string, string> Query { get; } = new();
    public Dictionary<string, string> RouteParams { get; } = new();
    public byte[] Body { get; set; } = Array.Empty<byte>();
    public string BodyText => System.Text.Encoding.UTF8.GetString(Body);
}
