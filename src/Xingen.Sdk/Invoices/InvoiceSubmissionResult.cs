namespace Xingen.Sdk.Invoices;

/// <summary>The <c>202 Accepted</c> response every submit/validate endpoint returns.</summary>
public sealed record InvoiceSubmissionResult
{
    public string? Id { get; init; }
    public InvoiceStatus Status { get; init; }
}
