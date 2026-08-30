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
    private readonly TcpListener _listener;
    private readonly Router _router = new();
    private readonly MiddlewarePipeline _pipeline = new();
    private CancellationTokenSource? _cts;

    public Router Router => _router;
    public MiddlewarePipeline Pipeline => _pipeline;

    public WebServer(IPAddress ip, int port)
    {
        _ip = ip;
        _port = port;
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
            return HttpResponse.Text("404 Not Found", 404);
        });

        var connectionHandler = new ConnectionHandler(handlerPipeline);

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
