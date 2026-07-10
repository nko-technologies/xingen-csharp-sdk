using Xingen.Sdk.Invoices;

namespace Xingen.Sdk.Errors;

/// <summary>
/// Thrown by a <c>*AndWaitAsync</c> polling helper when the configured <see cref="PollOptions"/>
/// timeout elapses before the invoice reaches a terminal status. The last known state is still
/// reachable via <see cref="PartialResult"/>.
/// </summary>
public class XingenTimeoutException : XingenException
{
    public InvoiceRecord PartialResult { get; }

    public XingenTimeoutException(string message, InvoiceRecord partialResult)
        : base(message)
    {
        PartialResult = partialResult;
    }
}
