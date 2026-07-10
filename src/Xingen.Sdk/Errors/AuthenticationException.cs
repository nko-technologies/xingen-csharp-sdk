namespace Xingen.Sdk.Errors;

/// <summary>401 — missing or invalid API key. The backend returns no application-level body for this status.</summary>
public class AuthenticationException : ApiException
{
    public AuthenticationException(string message, string rawBody)
        : base(message, 401, null, rawBody) { }
}
