using System.Net.Sockets;
using MiniWebServer.Http;
using MiniWebServer.Middleware;

namespace MiniWebServer.Server;

public class ConnectionHandler
{
    private readonly RequestDelegate _pipeline;
    private readonly int _keepAliveTimeoutMs;
    private readonly int _maxRequestsPerConnection;

    public ConnectionHandler(RequestDelegate pipeline, int keepAliveTimeoutMs = 5000, int maxRequestsPerConnection = 100)
    {
        _pipeline = pipeline;
        _keepAliveTimeoutMs = keepAliveTimeoutMs;
        _maxRequestsPerConnection = maxRequestsPerConnection;
    }

    public async Task HandleConnectionAsync(TcpClient client, CancellationToken cancellationToken = default)
    {
        using (client)
        using (NetworkStream stream = client.GetStream())
        {
            int requestCount = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                // HTTP/1.1 connections are persistent by default: after serving a
                // request we keep reading until the client closes or asks us to close.
                stream.ReadTimeout = _keepAliveTimeoutMs;

                HttpRequest? request;
                try
                {
                    request = await HttpParser.ParseAsync(stream, cancellationToken);
                }
                catch (Exception ex) when (ex is IOException or ObjectDisposedException)
                {
                    // Client dropped the connection or went idle past the timeout.
                    break;
                }

                if (request == null)
                {
                    break;
                }

                requestCount++;
                bool forceClose = requestCount >= _maxRequestsPerConnection;

                HttpResponse response = await _pipeline(request);
                bool keepAlive = !forceClose && ShouldKeepAlive(request, response);

                if (keepAlive)
                {
                    response.Headers["Connection"] = "keep-alive";
                }
                else
                {
                    response.Headers["Connection"] = "close";
                }

                // HEAD responses carry the same headers as GET (including
                // Content-Length) but must not include a body.
                byte[] responseBytes = response.ToBytes(omitBody: request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase));
                await stream.WriteAsync(responseBytes, cancellationToken);
                await stream.FlushAsync(cancellationToken);

                if (!keepAlive)
                {
                    break;
                }
            }
        }
    }

    private static bool ShouldKeepAlive(HttpRequest request, HttpResponse response)
    {
        if (string.Equals(response.Headers.Get("Connection"), "close", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        if (string.Equals(request.Headers.Get("Connection"), "close", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return string.Equals(request.HttpVersion, "HTTP/1.1", StringComparison.OrdinalIgnoreCase);
    }
}