using System.IO;
using System.Text;
using System.Web;

namespace MiniWebServer.Http;

public static class HttpParser
{
    public static async Task<HttpRequest?> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var headerBuilder = new StringBuilder();
        int b;
        while ((b = stream.ReadByte()) != -1)
        {
            headerBuilder.Append((char)b);
            if (headerBuilder.Length >= 4 && headerBuilder.ToString(headerBuilder.Length - 4, 4) == "\r\n\r\n")
            {
                break;
            }
            if (headerBuilder.Length > 16384)
            {
                throw new InvalidOperationException("Headers too large");
            }
        }

        if (headerBuilder.Length == 0) return null;

        string headerText = headerBuilder.ToString();
        string[] lines = headerText.Split("\r\n", StringSplitOptions.None);
        if (lines.Length == 0 || string.IsNullOrWhiteSpace(lines[0])) return null;

        string[] requestLineParts = lines[0].Split(' ');
        if (requestLineParts.Length < 3) return null;

        var request = new HttpRequest
        {
            Method = requestLineParts[0].ToUpperInvariant(),
            RawUrl = requestLineParts[1],
            HttpVersion = requestLineParts[2]
        };

        int queryIndex = request.RawUrl.IndexOf('?');
        if (queryIndex >= 0)
        {
            request.Path = request.RawUrl[..queryIndex];
            string queryString = request.RawUrl[(queryIndex + 1)..];
            var parsedQuery = HttpUtility.ParseQueryString(queryString);
            foreach (string? key in parsedQuery.AllKeys)
            {
                if (key != null)
                {
                    request.Query[key] = parsedQuery[key] ?? "";
                }
            }
        }
        else
        {
            request.Path = request.RawUrl;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line)) break;
            int colonIndex = line.IndexOf(':');
            if (colonIndex > 0)
            {
                string key = line[..colonIndex].Trim();
                string val = line[(colonIndex + 1)..].Trim();
                request.Headers.Add(key, val);
            }
        }

        if (int.TryParse(request.Headers.Get("Content-Length"), out int contentLength) && contentLength > 0)
        {
            byte[] bodyBuffer = new byte[contentLength];
            int totalRead = 0;
            while (totalRead < contentLength)
            {
                int read = await stream.ReadAsync(bodyBuffer.AsMemory(totalRead, contentLength - totalRead), cancellationToken);
                if (read == 0) break;
                totalRead += read;
            }
            request.Body = bodyBuffer;

            string contentType = request.Headers.Get("Content-Type") ?? "";
            if (contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                var parsedForm = HttpUtility.ParseQueryString(request.BodyText);
                foreach (string? key in parsedForm.AllKeys)
                {
                    if (key != null)
                    {
                        request.Form[key] = parsedForm[key] ?? "";
                    }
                }
            }
        }

        return request;
    }
}
