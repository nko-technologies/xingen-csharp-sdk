using Xingen.Sdk.Errors;

namespace Xingen.Sdk.Http;

/// <summary>Wraps <see cref="IHttpTransport.SendAsync"/> so every resource client reports transport failures the same way.</summary>
internal static class Requests
{
    public static async Task<HttpResponseMessage> SendAsync(
        IHttpTransport transport, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await transport.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            throw new XingenIOException($"Request to {request.RequestUri} failed", e);
        }
        catch (TaskCanceledException e) when (!cancellationToken.IsCancellationRequested)
        {
            // The HttpClient-level request timeout elapsed — this is not a caller-initiated cancellation.
            throw new XingenIOException($"Request to {request.RequestUri} timed out", e);
        }
    }
}
