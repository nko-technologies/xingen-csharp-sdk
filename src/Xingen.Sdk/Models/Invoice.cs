namespace Xingen.Sdk.Models;

/// <summary>Read-only invoice model, as returned inside an <see cref="Invoices.InvoiceRecord"/>.</summary>
public sealed record Invoice
{
    public string? InvoiceNumber { get; init; }
    public DateOnly? IssueDate { get; init; }
    public DateOnly? DueDate { get; init; }
    public DateOnly? TaxPointDate { get; init; }
    public string? Currency { get; init; }
    public string? BuyerReference { get; init; }
    public string? SpecificationId { get; init; }
    public string? ProfileId { get; init; }
    public string? TypeCode { get; init; }
    public string? OrderReference { get; init; }
    public string? SalesOrderReference { get; init; }
    public string? ProjectReference { get; init; }
    public string? ContractReference { get; init; }
    public string? ReceivingAdviceReference { get; init; }
    public string? DespatchAdviceReference { get; init; }
    public string? TenderOrLotReference { get; init; }
    public string? InvoicedObjectId { get; init; }
    public string? InvoicedObjectSchemeId { get; init; }
    public string? BuyerAccountingReference { get; init; }
    public List<string> Notes { get; init; } = [];
    public string? PaymentTermsNote { get; init; }
    public List<PrecedingInvoiceReference> PrecedingInvoiceReferences { get; init; } = [];
    public List<SupportingDocument> SupportingDocuments { get; init; } = [];
    public int ProjectReferenceCount { get; init; }
    public DateOnly? DeliveryPeriodStart { get; init; }
    public DateOnly? DeliveryPeriodEnd { get; init; }

    /// <summary>Null iff no document-level invoicing period was present in the source document.</summary>
    public InvoicePeriod? InvoicePeriod { get; init; }

    /// <summary>Null iff no delivery element was present in the source document.</summary>
    public Delivery? Delivery { get; init; }

    public Party? Supplier { get; init; }
    public Party? Buyer { get; init; }

    /// <summary>Null unless the payee differs from the seller.</summary>
    public Party? Payee { get; init; }

    /// <summary>Null unless a tax representative is present.</summary>
    public Party? TaxRepresentative { get; init; }

    public List<InvoiceLine> Lines { get; init; } = [];
    public List<TaxBreakdown> TaxBreakdowns { get; init; } = [];
    public List<AllowanceCharge> AllowanceCharges { get; init; } = [];
    public List<PaymentMeans> PaymentMeans { get; init; } = [];

    public string? TaxCurrencyCode { get; init; }
    public int TaxTotalWithSubtotalsCount { get; init; }
    public int TaxTotalWithoutSubtotalsCount { get; init; }

    public decimal? LineExtensionAmount { get; init; }
    public decimal? AllowanceTotalAmount { get; init; }
    public decimal? ChargeTotalAmount { get; init; }
    public decimal? TaxExclusiveAmount { get; init; }
    public decimal? TaxAmount { get; init; }
    public decimal? TaxAmountInAccountingCurrency { get; init; }
    public decimal? TaxInclusiveAmount { get; init; }
    public decimal? PrepaidAmount { get; init; }
    public decimal? PayableRoundingAmount { get; init; }
    public decimal? PayableAmount { get; init; }
}
