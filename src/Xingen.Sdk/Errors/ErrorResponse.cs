using System.Text.Json.Serialization;

namespace Xingen.Sdk.Errors;

/// <summary>
/// Mirrors the backend's standard error body shape. Present on 400/403/404/500 responses.
/// <b>Not</b> present on 429 (see <see cref="QuotaExceededException"/>) or 401, which use a
/// different or empty body.
/// </summary>
public sealed class ErrorResponse
{
    public string? Message { get; init; }

    public string? Error { get; init; }

    public int Code { get; init; }

    public DateTimeOffset? Timestamp { get; init; }

    [JsonPropertyName("fieldErrors")]
    public Dictionary<string, string>? FieldErrors { get; init; }
}
