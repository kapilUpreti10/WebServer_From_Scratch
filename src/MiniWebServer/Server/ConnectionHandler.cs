using System.Net.Sockets;
using MiniWebServer.Http;
using MiniWebServer.Middleware;

namespace MiniWebServer.Server;

public class ConnectionHandler
{
    private readonly RequestDelegate _pipeline;

    public ConnectionHandler(RequestDelegate pipeline)
    {
        _pipeline = pipeline;
    }

    public async Task HandleConnectionAsync(TcpClient client, CancellationToken cancellationToken = default)
    {
        using (client)
        using (NetworkStream stream = client.GetStream())
        {
            HttpRequest? request = await HttpParser.ParseAsync(stream, cancellationToken);
            if (request == null)
            {
                var badResponse = HttpResponse.Text("400 Bad Request", 400);
                byte[] badBytes = badResponse.ToBytes();
                await stream.WriteAsync(badBytes, cancellationToken);
                return;
            }

            HttpResponse response = await _pipeline(request);
            byte[] responseBytes = response.ToBytes();
            await stream.WriteAsync(responseBytes, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
    }
}
