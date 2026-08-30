using System.Collections;
using System.Collections.Generic;

namespace MiniWebServer.Http;

public class HttpHeaders : IEnumerable<KeyValuePair<string, string>>
{
    private readonly Dictionary<string, string> _headers = new(StringComparer.OrdinalIgnoreCase);

    public void Add(string key, string value) => _headers[key] = value;
    public bool ContainsKey(string key) => _headers.ContainsKey(key);
    public string? Get(string key) => _headers.TryGetValue(key, out var val) ? val : null;
    public string this[string key]
    {
        get => _headers[key];
        set => _headers[key] = value;
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _headers.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
