namespace Xingen.Sdk.Errors;

/// <summary>404 — the requested resource does not exist.</summary>
public class NotFoundException : ApiException
{
    public NotFoundException(string message, ErrorResponse? errorResponse, string rawBody)
        : base(message, 404, errorResponse, rawBody) { }
}
