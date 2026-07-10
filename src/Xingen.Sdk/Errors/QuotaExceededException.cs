namespace Xingen.Sdk.Errors;

/// <summary>
/// 429 — the API key's request quota has been exhausted. This bypasses the backend's normal error
/// pipeline, so the body is <c>{"error": "..."}</c>, not the standard <see cref="ErrorResponse"/> shape.
/// </summary>
public class QuotaExceededException : ApiException
{
    public QuotaExceededException(string message, string rawBody)
        : base(message, 429, null, rawBody) { }
}
