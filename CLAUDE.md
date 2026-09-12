# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Project Is

An educational HTTP/1.1 web server built from scratch in C# using raw TCP sockets (`TcpListener`, `TcpClient`, `NetworkStream`). No ASP.NET Core, no Kestrel, no HTTP server libraries. The goal is to understand exactly what happens underneath production web frameworks.

## Build & Test Commands

```bash
dotnet build                          # Build the solution
dotnet test                           # Run all xUnit tests
dotnet test --filter "FullyQualifiedName~ClassName"  # Run a single test class
dotnet test --filter "DisplayName=TestName"           # Run a single test
dotnet run --project src/MiniWebServer                # Start the server
```

## Architecture

```
src/MiniWebServer/          ← Console app (the server engine + host)
tests/MiniWebServer.Tests/  ← xUnit tests (unit + integration)
```

### Core Components (building incrementally across 20 phases)

- **Server/** — `TcpListener` listens on a port, accepts TCP connections, hands them off to a `ConnectionHandler` which manages the read/parse/respond/write lifecycle per client.
- **Http/** — `HttpParser` converts raw TCP bytes into `HttpRequest` objects (request line + headers + body). `HttpResponse` is serialized back to raw HTTP bytes. `HttpHeaders` is a case-insensitive header dictionary.
- **Routing/** — `Router` matches `(Method, Path)` tuples to handler delegates. Supports parameterized routes like `/users/{id}`.
- **Middleware/** — Pipeline pattern: request flows through a chain of middleware (logging, error handling, static files) before reaching the route handler.
- **StaticFiles/** — Serves files from a configured directory with MIME type detection and path traversal protection.

### Key Data Flow

```
TCP bytes → HttpParser → HttpRequest → Middleware → Router → Handler
    → HttpResponse → HTTP bytes → TCP stream → Client
```

## Design Constraints

- C# and .NET only. No third-party HTTP/server libraries.
- Standard .NET APIs are allowed: `TcpListener`, `TcpClient`, `NetworkStream`, `System.Text.Json`, `File` APIs.
- Target framework: .NET 9.0.
- HTTP/1.1 only (no HTTP/2, no TLS/HTTPS, no chunked transfer encoding).
- Prefer clarity over performance. Optimize only after measuring.
- Write tests as features are introduced.

## Project Conventions

- Top-level `Program.cs` is the server entry point.
- Test file names mirror the component they test (e.g., `HttpParserTests.cs` tests `HttpParser.cs`).
- Integration tests use `HttpClient` against a real running server instance.
