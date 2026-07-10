using System.Net.Http.Headers;
using Xingen.Sdk.Http;

namespace Xingen.Sdk.ApiKeys;

/// <summary>Create, list, and revoke API keys. Reachable via <see cref="XingenClient.ApiKeys"/>.</summary>
public sealed class ApiKeysClient
{
    private const string BasePath = "/v1/api-keys";

    private readonly IHttpTransport _transport;
    private readonly RequestBuilder _requestBuilder;
    private readonly JsonCodec _json;

    public ApiKeysClient(IHttpTransport transport, RequestBuilder requestBuilder, JsonCodec json)
    {
        _transport = transport;
        _requestBuilder = requestBuilder;
        _json = json;
    }

    /// <summary>The <see cref="CreatedApiKey.RawKey"/> on the result is shown only this once — persist it immediately.</summary>
    public async Task<CreatedApiKey> CreateAsync(CreateApiKeyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var httpRequest = _requestBuilder.NewRequest(BasePath);
        httpRequest.Method = HttpMethod.Post;
        httpRequest.Content = new ByteArrayContent(_json.Encode(request));
        httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await Requests.SendAsync(_transport, httpRequest, cancellationToken).ConfigureAwait(false);
        return await ResponseHandler.DecodeOrThrowAsync<CreatedApiKey>(response, _json).ConfigureAwait(false);
    }

    public async Task<List<ApiKey>> ListAsync(CancellationToken cancellationToken = default)
    {
        var request = _requestBuilder.NewRequest(BasePath);
        var response = await Requests.SendAsync(_transport, request, cancellationToken).ConfigureAwait(false);
        return await ResponseHandler.DecodeOrThrowAsync<List<ApiKey>>(response, _json).ConfigureAwait(false);
    }

    public async Task RevokeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = _requestBuilder.NewRequest($"{BasePath}/{id}");
        request.Method = HttpMethod.Delete;
        var response = await Requests.SendAsync(_transport, request, cancellationToken).ConfigureAwait(false);
        await ResponseHandler.RequireSuccessAsync(response, _json).ConfigureAwait(false);
    }
}
