# Core Concepts in MiniWebServer

Building an HTTP/1.1 web server from raw TCP sockets provides a deep understanding of low-level web architecture and .NET concepts.

---

## 1. Networking & Socket Fundamentals

### TCP vs HTTP
- **TCP (Transmission Control Protocol)** is a transport layer (Layer 4) byte stream protocol. It ensures reliable, ordered, error-checked delivery of stream data.
- **HTTP (Hypertext Transfer Protocol)** is an application layer (Layer 7) text-based protocol that runs on top of TCP.

### `TcpListener` & `TcpClient`
- `TcpListener`: Listens on an IP address and port (e.g. `127.0.0.1:8080`) and accepts incoming TCP connections.
- `TcpClient`: Represents the open socket connection to a client.
- `NetworkStream`: Provides access to read incoming raw bytes sent by client and write outgoing response bytes.

---

## 2. HTTP/1.1 Request & Response Format

### HTTP Request Specification
A standard HTTP request consists of:
```
<Method> <Path+Query> <HTTP-Version>\r\n
Header-Name: Header-Value\r\n
\r\n
[Optional Body Bytes]
```

### Parsing Logic
1. Read bytes from `NetworkStream` until finding `\r\n\r\n` (indicates end of headers).
2. Extract status line (`GET /users/10?format=json HTTP/1.1`).
3. Parse headers into a case-insensitive dictionary (`HttpHeaders`).
4. Read exact body length specified by `Content-Length` header bytes.

### HTTP Response Specification
```
HTTP/1.1 <StatusCode> <StatusReason>\r\n
Header-Name: Header-Value\r\n
\r\n
[Body Bytes]
```

---

## 3. Dynamic Routing & Regex Parameters

### Route Matching
Matches incoming `(Method, Path)` against configured patterns.
- Parameterized pattern: `/users/{id}`
- Internal Regex generated: `^/users/([^/]+)$`
- Parameters are extracted into `req.RouteParams["id"]`.

---

## 4. Middleware Pipeline Pattern

- Chain of Responsibility design pattern.
- Each middleware handles an incoming request and decides whether to pass execution to `next(request)` or short-circuit.
- Wraps request handling for cross-cutting concerns (Logging, Global Error Handling, Static File Serving).

---

## 5. Security: Path Traversal Defense

When serving static files from disk:
- Malicious request: `GET /../secret.txt`
- Defense: `Path.GetFullPath(...)` converts path to canonical absolute path and ensures `fullPath.StartsWith(rootPath)`. If false, returns `403 Forbidden`.

---

## 6. Asynchronous Non-Blocking Execution

- Uses `AcceptTcpClientAsync` and `Task.Run` so each TCP connection is handled asynchronously without blocking the listening loop.
- Utilizes `CancellationToken` for graceful shutdown.
