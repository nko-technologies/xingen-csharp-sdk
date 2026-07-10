namespace Xingen.Sdk.Errors;

/// <summary>Base type for every exception thrown by this SDK.</summary>
public class XingenException : Exception
{
    public XingenException(string message) : base(message) { }

    public XingenException(string message, Exception? innerException) : base(message, innerException) { }
}
