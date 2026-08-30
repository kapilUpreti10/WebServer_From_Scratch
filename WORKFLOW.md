# WebServer Architecture & Detailed Workflow

This document explains step-by-step how `MiniWebServer` processes an HTTP request and details the responsibility of each file and function.

---

## Request Execution Workflow

```
1. Client connects to http://127.0.0.1:8080
                     │
2. WebServer.cs      ──> Accepts TcpClient asynchronously
                     │
3. ConnectionHandler ──> Reads raw stream bytes
                     │
4. HttpParser.cs     ──> Converts bytes into HttpRequest object
                     │
5. Pipeline          ──> ErrorHandlingMiddleware
                     │     └── LoggingMiddleware
                     │          └── StaticFileMiddleware
                     │               └── Router.cs
                     │
6. Route Handler     ──> Generates HttpResponse
                     │
7. HttpResponse.cs   ──> Serializes response into HTTP/1.1 bytes
                     │
8. ConnectionHandler ──> Writes bytes back to NetworkStream
```

---

## File & Function Breakdown

### 1. `Server/WebServer.cs`
- **Purpose:** Manages server socket lifetime, accepts TCP clients, constructs middleware pipeline.
- **Key Methods:**
  - `Start()`: Begins listening on `TcpListener` and launches async accept loop.
  - `Stop()`: Cancels cancellation token and stops `TcpListener`.

### 2. `Server/ConnectionHandler.cs`
- **Purpose:** Reads raw socket stream, delegates parsing & execution, writes response bytes.
- **Key Methods:**
  - `HandleConnectionAsync(TcpClient client, CancellationToken ct)`: Handles stream lifecycle for a single TCP client.

### 3. `Http/HttpParser.cs`
- **Purpose:** Converts raw stream bytes into structured `HttpRequest`.
- **Key Methods:**
  - `ParseAsync(Stream stream)`: Reads headers up to `\r\n\r\n`, parses method/path/query/headers, and reads body bytes based on `Content-Length`.

### 4. `Http/HttpRequest.cs` & `Http/HttpResponse.cs`
- **Purpose:** Models HTTP entities.
- **Key Methods:**
  - `HttpResponse.ToBytes()`: Formats headers and body into valid HTTP response binary payload.

### 5. `Routing/Router.cs`
- **Purpose:** Maps standard HTTP verb and URL patterns to request handlers.
- **Key Methods:**
  - `AddRoute(method, pathPattern, handler)`: Converts parameter templates like `/users/{id}` into Regex patterns.
  - `Match(HttpRequest request)`: Tests request path against routes and populates `req.RouteParams`.

### 6. `Middleware/MiddlewarePipeline.cs`
- **Purpose:** Chains middleware instances together.
- **Key Methods:**
  - `Use(IMiddleware middleware)`: Registers middleware.
  - `Build(RequestDelegate fallbackHandler)`: Composes delegates into a single executable pipeline delegate.

### 7. `StaticFiles/StaticFileMiddleware.cs`
- **Purpose:** Serves physical files from disk with MIME detection and path traversal check.
- **Key Methods:**
  - `InvokeAsync(...)`: Checks file existence and safety, returning raw file bytes if matched.
