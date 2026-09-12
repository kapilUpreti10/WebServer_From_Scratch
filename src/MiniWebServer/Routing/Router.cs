using System.Text.RegularExpressions;
using MiniWebServer.Http;

namespace MiniWebServer.Routing;

public delegate Task<HttpResponse> RequestHandler(HttpRequest request);

public class Router
{
    private class RouteEntry
    {
        public string Method { get; set; } = "";
        public Regex PathRegex { get; set; } = null!;
        public List<string> ParameterNames { get; set; } = new();
        public Func<HttpRequest, Task<HttpResponse>> Handler { get; set; } = null!;
    }

    private readonly List<RouteEntry> _routes = new();

    public void AddRoute(string method, string pathPattern, Func<HttpRequest, Task<HttpResponse>> handler)
    {
        var paramNames = new List<string>();
        string pattern = "^" + Regex.Replace(pathPattern, @"\{([a-zA-Z0-9_]+)\}", m =>
        {
            paramNames.Add(m.Groups[1].Value);
            return @"([^/]+)";
        }) + "$";

        _routes.Add(new RouteEntry
        {
            Method = method.ToUpperInvariant(),
            PathRegex = new Regex(pattern, RegexOptions.IgnoreCase),
            ParameterNames = paramNames,
            Handler = handler
        });
    }

    public void Get(string path, Func<HttpRequest, Task<HttpResponse>> handler) => AddRoute("GET", path, handler);
    public void Post(string path, Func<HttpRequest, Task<HttpResponse>> handler) => AddRoute("POST", path, handler);
    public void Put(string path, Func<HttpRequest, Task<HttpResponse>> handler) => AddRoute("PUT", path, handler);
    public void Patch(string path, Func<HttpRequest, Task<HttpResponse>> handler) => AddRoute("PATCH", path, handler);
    public void Delete(string path, Func<HttpRequest, Task<HttpResponse>> handler) => AddRoute("DELETE", path, handler);

    public string[]? GetAllowedMethods(string path)
    {
        var methods = _routes
            .Where(r => r.PathRegex.IsMatch(path))
            .Select(r => r.Method)
            .Distinct()
            .ToList();
        if (methods.Count == 0) return null;

        // Any GET route also satisfies HEAD requests.
        if (methods.Contains("GET") && !methods.Contains("HEAD"))
        {
            methods.Add("HEAD");
        }
        methods.Sort();
        return methods.ToArray();
    }

    public Func<HttpRequest, Task<HttpResponse>>? Match(HttpRequest request)
    {
        foreach (var route in _routes)
        {
            if (MatchesMethod(route.Method, request.Method))
            {
                var match = route.PathRegex.Match(request.Path);
                if (match.Success)
                {
                    for (int i = 0; i < route.ParameterNames.Count; i++)
                    {
                        request.RouteParams[route.ParameterNames[i]] = match.Groups[i + 1].Value;
                    }
                    return route.Handler;
                }
            }
        }
        return null;
    }

    // HEAD requests reuse the GET handler; the server omits the response body.
    private static bool MatchesMethod(string routeMethod, string requestMethod) =>
        routeMethod == requestMethod ||
        (requestMethod.Equals("HEAD", StringComparison.OrdinalIgnoreCase) && routeMethod == "GET");
}
