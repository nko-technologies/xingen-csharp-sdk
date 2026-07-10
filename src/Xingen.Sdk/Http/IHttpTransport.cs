namespace Xingen.Sdk.Http;

/// <summary>
/// Sends a request and returns its raw response. Exists as a seam so tests can substitute a fake
/// transport instead of performing real network I/O.
/// </summary>
public interface IHttpTransport
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}
