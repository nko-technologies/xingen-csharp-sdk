using System.Net.Http.Headers;
using System.Text;

namespace Xingen.Sdk.Http;

/// <summary>
/// Builds <see cref="HttpRequestMessage"/>s against a fixed base URL, attaching auth/user-agent
/// headers and handling query-string encoding so callers never string-concatenate URLs by hand.
/// </summary>
public sealed class RequestBuilder
{
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly string _userAgent;

    public RequestBuilder(Uri baseUrl, string apiKey, string userAgent)
    {
        var normalized = baseUrl.ToString();
        _baseUrl = normalized.EndsWith('/') ? normalized[..^1] : normalized;
        _apiKey = apiKey;
        _userAgent = userAgent;
    }

    public HttpRequestMessage NewRequest(string path, QueryParams? queryParams = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(path, queryParams));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd(_userAgent);
        return request;
    }

    public static QueryParams Query() => new();

    private Uri BuildUri(string path, QueryParams? queryParams)
    {
        var sb = new StringBuilder(_baseUrl);
        sb.Append(path.StartsWith('/') ? path : "/" + path);
        var first = true;
        foreach (var (key, value) in queryParams?.Entries ?? [])
        {
            if (value is null)
            {
                continue;
            }
            sb.Append(first ? '?' : '&');
            sb.Append(Uri.EscapeDataString(key)).Append('=').Append(Uri.EscapeDataString(value));
            first = false;
        }
        return new Uri(sb.ToString());
    }

    /// <summary>Small ordered builder for query parameters, so call sites read as a fluent chain.</summary>
    public sealed class QueryParams
    {
        private readonly List<(string Key, string? Value)> _entries = [];

        internal IReadOnlyList<(string Key, string? Value)> Entries => _entries;

        public QueryParams Put(string key, string? value)
        {
            _entries.Add((key, value));
            return this;
        }

        public QueryParams Put(string key, object? value) => Put(key, value?.ToString());
    }
}
