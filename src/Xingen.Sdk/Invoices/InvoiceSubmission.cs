using Xingen.Sdk.Models;

namespace Xingen.Sdk.Invoices;

/// <summary>
/// Request body for <see cref="InvoicesClient.SubmitAsync"/>. Mirrors the backend's flat
/// <c>InvoiceRequest</c> shape — deliberately not the same class hierarchy as the much richer
/// <see cref="Invoice"/> read model, since submit and read are genuinely different contracts.
/// </summary>
public sealed record InvoiceSubmission
{
    public required string InvoiceNumber { get; init; }
    public required DateOnly IssueDate { get; init; }
    public required string Currency { get; init; }

    /// <summary>Optional in general, mandatory in practice for XRechnung (Leitweg-ID).</summary>
    public string? BuyerReference { get; init; }

    public required ValidationProfile ValidationProfile { get; init; }
    public required PartyInput Supplier { get; init; }
    public required PartyInput Buyer { get; init; }
    public List<LineInput> Lines { get; init; } = [];

    /// <summary>Seller or buyer, as submitted with a new invoice.</summary>
    public sealed record PartyInput
    {
        public required string Name { get; init; }
        public string? VatId { get; init; }
        public string? LeitwegId { get; init; }
    }

    /// <summary>A single invoice line, as submitted with a new invoice.</summary>
    public sealed record LineInput
    {
        public required string Description { get; init; }
        public required decimal Quantity { get; init; }
        public required string Unit { get; init; }
        public required decimal Price { get; init; }
        public required decimal TaxRate { get; init; }
    }
}
