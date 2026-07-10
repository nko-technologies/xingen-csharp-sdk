namespace Xingen.Sdk.Models;

public sealed record InvoiceLine
{
    public string? LineId { get; init; }
    public string? Note { get; init; }
    public string? ObjectId { get; init; }
    public string? ObjectIdSchemeId { get; init; }
    public string? OrderLineReference { get; init; }
    public string? AccountingReference { get; init; }
    public string? ItemName { get; init; }
    public string? Description { get; init; }
    public string? SellerItemId { get; init; }
    public string? BuyerItemId { get; init; }
    public string? StandardItemId { get; init; }
    public string? StandardItemIdSchemeId { get; init; }
    public string? OriginCountryCode { get; init; }
    public List<ItemClassification> Classifications { get; init; } = [];
    public List<ItemAttribute> Attributes { get; init; } = [];
    public decimal? Quantity { get; init; }
    public string? Unit { get; init; }
    public decimal? Price { get; init; }
    public decimal? GrossPrice { get; init; }
    public decimal? PriceDiscount { get; init; }
    public bool PriceHasCharge { get; init; }
    public decimal? PriceBaseQuantity { get; init; }
    public string? PriceBaseQuantityUnit { get; init; }
    public string? TaxCategoryCode { get; init; }
    public decimal? TaxRate { get; init; }
    public decimal? LineNetAmount { get; init; }

    /// <summary>Null iff no line-level invoicing period was present in the source document.</summary>
    public InvoicePeriod? Period { get; init; }

    public int DocumentReferenceCount { get; init; }
    public string? DocumentReferenceTypeCode { get; init; }
    public List<LineAllowanceCharge> AllowanceCharges { get; init; } = [];
}
