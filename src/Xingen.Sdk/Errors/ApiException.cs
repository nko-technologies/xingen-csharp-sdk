namespace Xingen.Sdk.Errors;

/// <summary>Any HTTP response the SDK understands as an error (4xx/5xx). Subtyped for the common statuses.</summary>
public class ApiException : XingenException
{
    public int StatusCode { get; }

    /// <summary>Null if the response body didn't match the expected shape (e.g. an unexpected 5xx from a proxy).</summary>
    public ErrorResponse? ErrorResponse { get; }

    /// <summary>The raw response body, always retained even if it could not be parsed as <see cref="Errors.ErrorResponse"/>.</summary>
    public string RawBody { get; }

    public ApiException(string message, int statusCode, ErrorResponse? errorResponse, string rawBody)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorResponse = errorResponse;
        RawBody = rawBody;
    }
}
