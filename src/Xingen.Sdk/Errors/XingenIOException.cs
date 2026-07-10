namespace Xingen.Sdk.Errors;

/// <summary>Wraps a network/transport-level failure (connection refused, DNS failure, etc.) — not an HTTP error response.</summary>
public class XingenIOException : XingenException
{
    public XingenIOException(string message, Exception innerException)
        : base(message, innerException) { }
}
