using System.Net;
using System.Net.Sockets;
using MiniWebServer.Http;
using MiniWebServer.Middleware;
using MiniWebServer.Routing;

namespace MiniWebServer.Server;

public class WebServer
{
    private readonly IPAddress _ip;
    private readonly int _port;
    private readonly int _keepAliveTimeoutMs;
    private readonly int _maxRequestsPerConnection;
    private readonly TcpListener _listener;
    private readonly Router _router = new();
    private readonly MiddlewarePipeline _pipeline = new();
    private CancellationTokenSource? _cts;

    public Router Router => _router;
    public MiddlewarePipeline Pipeline => _pipeline;

    public WebServer(IPAddress ip, int port, int keepAliveTimeoutMs = 5000, int maxRequestsPerConnection = 100)
    {
        _ip = ip;
        _port = port;
        _keepAliveTimeoutMs = keepAliveTimeoutMs;
        _maxRequestsPerConnection = maxRequestsPerConnection;
        _listener = new TcpListener(ip, port);
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _listener.Start();
        Console.WriteLine($"Server running on http://{_ip}:{_port}");

        var handlerPipeline = _pipeline.Build(async (request) =>
        {
            var routeHandler = _router.Match(request);
            if (routeHandler != null)
            {
                return await routeHandler(request);
            }

            // The path exists but not for this method -> 405 Method Not Allowed.
            string[]? allowed = _router.GetAllowedMethods(request.Path);
            if (allowed is { Length: > 0 })
            {
                var response = HttpResponse.Text("405 Method Not Allowed", 405);
                response.Headers["Allow"] = string.Join(", ", allowed);
                return response;
            }

            return HttpResponse.Text("404 Not Found", 404);
        });

        var connectionHandler = new ConnectionHandler(handlerPipeline, _keepAliveTimeoutMs, _maxRequestsPerConnection);

        Task.Run(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync(_cts.Token);
                    _ = connectionHandler.HandleConnectionAsync(client, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Accept error: {ex.Message}");
                }
            }
        });
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener.Stop();
        Console.WriteLine("Server stopped.");
    }
}
