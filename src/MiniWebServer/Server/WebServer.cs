using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MiniWebServer.Server;

/// <summary>
/// Phase 2: A minimal TCP server that listens on an IP/port, accepts a single
/// connection, and sends back a hardcoded HTTP response.
///
/// NOTE: This is intentionally synchronous and single-client for now.
/// Async + concurrency arrive in Phase 8.
/// </summary>
public class WebServer
{
    private readonly IPAddress _ip;
    private readonly int _port;
    private readonly TcpListener _listener;

    public WebServer(IPAddress ip, int port)
    {
        _ip = ip;
        _port = port;
        _listener = new TcpListener(ip, port);
    }

    /// <summary>
    /// Start listening and accept exactly one connection, respond to it, and stop.
    /// </summary>
    public void Start()
    {
        _listener.Start();
        Console.WriteLine($"Listening on http://{_ip}:{_port}");

        // Accept ONE connection (blocks until a client connects).
        // In Phase 8 this becomes an async loop serving many clients.
        TcpClient client = _listener.AcceptTcpClient();
        Console.WriteLine("Client connected!");

        using (client)
        using (NetworkStream stream = client.GetStream())
        {
            // Read whatever bytes the client sent (may be empty, that's fine).
            byte[] buffer = new byte[4096];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            Console.WriteLine($"Received {bytesRead} bytes from client.");

            // Build a hardcoded HTTP response as text, then encode it to bytes.
            string responseText =
                "HTTP/1.1 200 OK\r\n" +
                "Content-Type: text/plain\r\n" +
                "Content-Length: 13\r\n" +
                "\r\n" +
                "Hello, Kapil!";

            byte[] responseBytes = Encoding.UTF8.GetBytes(responseText);
            stream.Write(responseBytes, 0, responseBytes.Length);
            stream.Flush();
            Console.WriteLine($"Sent {responseBytes.Length} bytes to client.");
        }

        _listener.Stop();
        Console.WriteLine("Server stopped.");
    }
}
