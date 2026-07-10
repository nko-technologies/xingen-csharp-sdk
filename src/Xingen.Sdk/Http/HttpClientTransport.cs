namespace Xingen.Sdk.Http;

public sealed class HttpClientTransport : IHttpTransport
{
    private readonly HttpClient _httpClient;

    public HttpClientTransport(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        _httpClient.SendAsync(request, cancellationToken);
}
