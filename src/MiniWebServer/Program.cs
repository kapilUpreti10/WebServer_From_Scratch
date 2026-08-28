using System.Net;
using MiniWebServer.Server;

// Bind to the loopback address (localhost only) on port 8080.
WebServer server = new WebServer(IPAddress.Loopback, 8080);
server.Start();
