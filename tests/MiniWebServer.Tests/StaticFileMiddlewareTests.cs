using MiniWebServer.Http;
using MiniWebServer.StaticFiles;

namespace MiniWebServer.Tests;

public class StaticFileMiddlewareTests
{
    [Fact]
    public async Task ServesExistingFile()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        string file = Path.Combine(tempDir, "test.txt");
        await File.WriteAllTextAsync(file, "hello static");

        try
        {
            var middleware = new StaticFileMiddleware(tempDir);
            var req = new HttpRequest { Method = "GET", Path = "/test.txt" };
            var response = await middleware.InvokeAsync(req, (r) => Task.FromResult(HttpResponse.Text("Fallback", 404)));

            Assert.Equal(200, response.StatusCode);
            Assert.Equal("hello static", System.Text.Encoding.UTF8.GetString(response.Body));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task PathTraversal_ReturnsForbidden()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            var middleware = new StaticFileMiddleware(tempDir);
            var req = new HttpRequest { Method = "GET", Path = "/../secret.txt" };
            var response = await middleware.InvokeAsync(req, (r) => Task.FromResult(HttpResponse.Text("Fallback", 404)));

            Assert.Equal(403, response.StatusCode);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
