using Xingen.Sdk.Errors;

namespace Xingen.Sdk.Http;

/// <summary>
/// Centralizes HTTP status -&gt; exception mapping so every resource client handles errors the same
/// way. Two shapes never go through the normal <see cref="ErrorResponse"/> parse path: 429 (quota,
/// written raw by a security filter ahead of the backend's exception pipeline) and 401 (no
/// application-level body at all). Parsing never throws a secondary exception that could mask the
/// real HTTP error — callers always get a typed <see cref="ApiException"/> with the raw body attached.
/// </summary>
internal static class ResponseHandler
{
    public static async Task<T> DecodeOrThrowAsync<T>(HttpResponseMessage response, JsonCodec codec)
    {
        var body = await RequireSuccessAsync(response, codec).ConfigureAwait(false);
        return codec.Decode<T>(body);
    }

    public static Task<byte[]> BytesOrThrowAsync(HttpResponseMessage response, JsonCodec codec) =>
        RequireSuccessAsync(response, codec);

    public static async Task<byte[]> RequireSuccessAsync(HttpResponseMessage response, JsonCodec codec)
    {
        var body = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        var status = (int)response.StatusCode;
        if (status is >= 200 and < 300)
        {
            return body;
        }
        throw ToException(status, body, codec);
    }

    private static ApiException ToException(int status, byte[] body, JsonCodec codec)
    {
        var raw = JsonCodec.BodyAsString(body);

        if (status == 429)
        {
            var message = codec.TryDecodeField(body, "error") ?? "Quota exceeded";
            return new QuotaExceededException(message, raw);
        }
        if (status == 401)
        {
            return new AuthenticationException("Authentication failed — check your API key", raw);
        }

        var errorResponse = codec.TryDecode<ErrorResponse>(body);
        var errorMessage = ErrorMessage(errorResponse, raw, status);

        return status switch
        {
            403 => new PermissionException(errorMessage, errorResponse, raw),
            404 => new NotFoundException(errorMessage, errorResponse, raw),
            400 => new ValidationRequestException(errorMessage, errorResponse, raw),
            _ => new ApiException(errorMessage, status, errorResponse, raw),
        };
    }

    private static string ErrorMessage(ErrorResponse? errorResponse, string raw, int status)
    {
        if (errorResponse?.Message is not null)
        {
            return errorResponse.Message;
        }
        return string.IsNullOrWhiteSpace(raw) ? $"Request failed with status {status}" : raw;
    }
}
