namespace Xingen.Sdk.Errors;

/// <summary>403 — e.g. the requested invoice exists but is not owned by the caller.</summary>
public class PermissionException : ApiException
{
    public PermissionException(string message, ErrorResponse? errorResponse, string rawBody)
        : base(message, 403, errorResponse, rawBody) { }
}
