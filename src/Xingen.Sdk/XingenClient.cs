using Xingen.Sdk.ApiKeys;
using Xingen.Sdk.Http;
using Xingen.Sdk.Invoices;

namespace Xingen.Sdk;

/// <summary>
/// Entry point for the Xingen API. Construct directly, then use <see cref="Invoices"/> and
/// <see cref="ApiKeys"/> to reach the resource-specific clients. A single <see cref="XingenClient"/>
/// holds one connection-pooled <see cref="HttpClient"/> and should be reused across calls rather
/// than rebuilt per request — dispose it (or let your DI container dispose it) once you're done
/// with it.
/// </summary>
public sealed class XingenClient : IDisposable
{
    private static readonly Uri DefaultBaseUrl = new("https://app.xingen.de/api");
    private static readonly TimeSpan DefaultConnectTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(30);

    private readonly HttpClient _httpClient;

    public InvoicesClient Invoices { get; }
    public ApiKeysClient ApiKeys { get; }

    /// <param name="apiKey">Required. An <c>xgn_live_</c>/<c>xgn_test_</c> prefixed API key.</param>
    /// <param name="baseUrl">Overrides the default <c>https://app.xingen.de/api</c> base URL — useful for self-hosted or local testing.</param>
    /// <param name="connectTimeout">Timeout for establishing the TCP/TLS connection. Defaults to 10 seconds.</param>
    /// <param name="requestTimeout">
    /// Per-request timeout, applied to every call the SDK makes (not the total time budget of the
    /// <c>*AndWaitAsync</c> polling helpers — see <see cref="PollOptions.Timeout"/> for that).
    /// Defaults to 30 seconds.
    /// </param>
    public XingenClient(string apiKey, Uri? baseUrl = null, TimeSpan? connectTimeout = null, TimeSpan? requestTimeout = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        var handler = new SocketsHttpHandler
        {
            ConnectTimeout = connectTimeout ?? DefaultConnectTimeout,
        };
        _httpClient = new HttpClient(handler)
        {
            Timeout = requestTimeout ?? DefaultRequestTimeout,
        };

        var transport = new HttpClientTransport(_httpClient);
        var json = new JsonCodec();
        var userAgent = $"xingen-csharp-sdk/{SdkVersion()} (.NET/{Environment.Version})";
        var requestBuilder = new RequestBuilder(baseUrl ?? DefaultBaseUrl, apiKey, userAgent);

        Invoices = new InvoicesClient(transport, requestBuilder, json);
        ApiKeys = new ApiKeysClient(transport, requestBuilder, json);
    }

    private static string SdkVersion()
    {
        var version = typeof(XingenClient).Assembly.GetName().Version;
        return version is null ? "dev" : version.ToString(3);
    }

    /// <summary>Disposes the underlying <see cref="HttpClient"/>.</summary>
    public void Dispose() => _httpClient.Dispose();
}
