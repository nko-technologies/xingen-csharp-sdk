namespace Xingen.Sdk.Errors;

/// <summary>400 — malformed request body or bean-validation failure.</summary>
public class ValidationRequestException : ApiException
{
    /// <summary>
    /// Empty unless the failure was a bean-validation error on a request body (as opposed to a
    /// plain <see cref="ErrorResponse"/>).
    /// </summary>
    public IReadOnlyDictionary<string, string> FieldErrors { get; }

    public ValidationRequestException(string message, ErrorResponse? errorResponse, string rawBody)
        : base(message, 400, errorResponse, rawBody)
    {
        FieldErrors = errorResponse?.FieldErrors ?? new Dictionary<string, string>();
    }
}
