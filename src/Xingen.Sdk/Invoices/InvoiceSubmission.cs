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

    /// <summary>Payment due date (BT-9). Either this or PaymentTermsNote is required whenever the payable amount is positive.</summary>
    public DateOnly? DueDate { get; init; }

    /// <summary>Value added tax point date (BT-7).</summary>
    public DateOnly? TaxPointDate { get; init; }

    public required string Currency { get; init; }

    /// <summary>VAT accounting currency code (BT-6), if different from Currency.</summary>
    public string? TaxCurrencyCode { get; init; }

    /// <summary>Optional in general, mandatory in practice for XRechnung (Leitweg-ID).</summary>
    public string? BuyerReference { get; init; }

    /// <summary>Payment terms (BT-20). Either this or DueDate is required whenever the payable amount is positive.</summary>
    public string? PaymentTermsNote { get; init; }

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
    public List<string>? Notes { get; init; }
    public List<PrecedingInvoiceReferenceInput>? PrecedingInvoiceReferences { get; init; }
    public List<SupportingDocumentInput>? SupportingDocuments { get; init; }
    public DateOnly? DeliveryPeriodStart { get; init; }
    public DateOnly? DeliveryPeriodEnd { get; init; }
    public InvoicePeriodInput? InvoicePeriod { get; init; }
    public DeliveryInput? Delivery { get; init; }

    public required ValidationProfile ValidationProfile { get; init; }
    public required PartyInput Supplier { get; init; }
    public required PartyInput Buyer { get; init; }

    /// <summary>Payee, if different from the seller (BG-10).</summary>
    public PartyInput? Payee { get; init; }

    /// <summary>Seller's tax representative (BG-11).</summary>
    public PartyInput? TaxRepresentative { get; init; }

    public List<LineInput> Lines { get; init; } = [];
    public List<PaymentMeansInput>? PaymentMeans { get; init; }
    public List<AllowanceChargeInput>? AllowanceCharges { get; init; }

    /// <summary>Seller, buyer, payee, or tax representative, as submitted with a new invoice.</summary>
    public sealed record PartyInput
    {
        public required string Name { get; init; }

        /// <summary>Legal registration name, if different from the trading name (BT-27/BT-44).</summary>
        public string? RegistrationName { get; init; }

        public string? VatId { get; init; }

        /// <summary>Tax registration identifier, non-VAT scheme (BT-32).</summary>
        public string? TaxRegistrationId { get; init; }

        /// <summary>Legal registration identifier (BT-30/BT-47).</summary>
        public string? LegalRegistrationId { get; init; }

        /// <summary>Legal registration identifier scheme (BT-30-1/BT-47-1).</summary>
        public string? LegalRegistrationSchemeId { get; init; }

        /// <summary>Additional legal information, e.g. legal form (BT-33).</summary>
        public string? AdditionalLegalInfo { get; init; }

        public string? LeitwegId { get; init; }

        /// <summary>Postal address (BG-5/BG-8) — mandatory for every profile for supplier/buyer; the backend rejects a party with no address.</summary>
        public AddressInput? Address { get; init; }

        public ContactInput? Contact { get; init; }
        public string? EndpointId { get; init; }
        public string? EndpointSchemeId { get; init; }
        public List<PartyIdentifierInput>? Identifiers { get; init; }
    }

    /// <summary>Postal address (BG-5/BG-8/BG-15) of a <see cref="PartyInput"/> or <see cref="DeliveryInput"/>. Only <see cref="CountryCode"/> is mandatory server-side.</summary>
    public sealed record AddressInput
    {
        public string? StreetName { get; init; }
        public string? AdditionalStreetName { get; init; }
        public string? AddressLine3 { get; init; }
        public string? City { get; init; }
        public string? PostalZone { get; init; }
        public string? CountrySubdivision { get; init; }
        public string? CountryCode { get; init; }
    }

    /// <summary>Contact details (BG-6/BG-9) of a <see cref="PartyInput"/>.</summary>
    public sealed record ContactInput
    {
        public string? Name { get; init; }
        public string? Telephone { get; init; }
        public string? Email { get; init; }
    }

    /// <summary>Additional party identifier (BT-29/BT-46/BT-60), e.g. a SEPA creditor identifier.</summary>
    public sealed record PartyIdentifierInput
    {
        public required string Id { get; init; }
        public string? SchemeId { get; init; }
    }

    /// <summary>Preceding invoice reference (BG-3) — e.g. the original invoice a credit note corrects.</summary>
    public sealed record PrecedingInvoiceReferenceInput
    {
        public required string Id { get; init; }
        public DateOnly? IssueDate { get; init; }
    }

    /// <summary>Additional supporting document (BG-24).</summary>
    public sealed record SupportingDocumentInput
    {
        public string? Id { get; init; }
        public string? SchemeId { get; init; }
        public string? TypeCode { get; init; }
        public string? Description { get; init; }
        public string? ExternalUri { get; init; }
        public string? MimeCode { get; init; }
        public string? Filename { get; init; }
    }

    /// <summary>Invoicing period (BG-14 document-level / BG-26 line-level).</summary>
    public sealed record InvoicePeriodInput
    {
        public DateOnly? StartDate { get; init; }
        public DateOnly? EndDate { get; init; }

        /// <summary>Tax point date code, UNTDID 2005 (BT-8, document level only).</summary>
        public string? DescriptionCode { get; init; }
    }

    /// <summary>Delivery information (BG-13).</summary>
    public sealed record DeliveryInput
    {
        public string? PartyName { get; init; }
        public string? LocationId { get; init; }
        public string? LocationSchemeId { get; init; }
        public AddressInput? Address { get; init; }
        public DateOnly? ActualDeliveryDate { get; init; }
    }

    /// <summary>Item classification (BT-158).</summary>
    public sealed record ItemClassificationInput
    {
        public required string Code { get; init; }
        public string? ListId { get; init; }
        public string? ListVersionId { get; init; }
    }

    /// <summary>Additional item attribute (BG-32).</summary>
    public sealed record ItemAttributeInput
    {
        public required string Name { get; init; }
        public required string Value { get; init; }
    }

    /// <summary>A single invoice line, as submitted with a new invoice.</summary>
    public sealed record LineInput
    {
        public required string Description { get; init; }

        /// <summary>Item name (BT-153), distinct from the free-text description if both are needed.</summary>
        public string? ItemName { get; init; }

        public string? Note { get; init; }
        public string? ObjectId { get; init; }
        public string? ObjectIdSchemeId { get; init; }
        public string? OrderLineReference { get; init; }
        public string? AccountingReference { get; init; }
        public string? SellerItemId { get; init; }
        public string? BuyerItemId { get; init; }
        public string? StandardItemId { get; init; }
        public string? StandardItemIdSchemeId { get; init; }
        public string? OriginCountryCode { get; init; }
        public List<ItemClassificationInput>? Classifications { get; init; }
        public List<ItemAttributeInput>? Attributes { get; init; }
        public required decimal Quantity { get; init; }
        public required string Unit { get; init; }
        public required decimal Price { get; init; }
        public decimal? GrossPrice { get; init; }
        public decimal? PriceDiscount { get; init; }
        public decimal? PriceBaseQuantity { get; init; }
        public string? PriceBaseQuantityUnit { get; init; }

        /// <summary>VAT category code, UNCL5305 (BT-151). Defaults to Standard rate if omitted.</summary>
        public string? TaxCategoryCode { get; init; }

        public required decimal TaxRate { get; init; }

        /// <summary>VAT exemption reason text (BT-120) — set when TaxCategoryCode is exempt/reverse-charge/out-of-scope.</summary>
        public string? ExemptionReason { get; init; }

        /// <summary>VAT exemption reason code, UNCL5305 (BT-121).</summary>
        public string? ExemptionReasonCode { get; init; }

        public InvoicePeriodInput? Period { get; init; }
        public List<LineAllowanceChargeInput>? AllowanceCharges { get; init; }
    }

    /// <summary>Payment means (BG-16).</summary>
    public sealed record PaymentMeansInput
    {
        public string? TypeCode { get; init; }
        public string? PaymentMeansText { get; init; }
        public string? RemittanceInformation { get; init; }
        public string? CreditTransferAccountId { get; init; }
        public string? AccountName { get; init; }
        public string? ServiceProviderId { get; init; }
        public string? MandateReferenceId { get; init; }
        public string? CardAccountNumber { get; init; }
        public string? CardHolderName { get; init; }
        public string? CreditorId { get; init; }
        public string? DebitedAccountId { get; init; }
    }

    /// <summary>Document-level allowance or charge (BG-20/BG-21).</summary>
    public sealed record AllowanceChargeInput
    {
        /// <summary>true = charge (BG-21), false = allowance (BG-20).</summary>
        public required bool Charge { get; init; }

        public required decimal Amount { get; init; }
        public decimal? BaseAmount { get; init; }
        public decimal? Percentage { get; init; }
        public string? VatCategoryCode { get; init; }
        public decimal? VatRate { get; init; }
        public string? Reason { get; init; }
        public string? ReasonCode { get; init; }
    }

    /// <summary>Line-level allowance or charge (BG-27/BG-28).</summary>
    public sealed record LineAllowanceChargeInput
    {
        public required bool Charge { get; init; }
        public required decimal Amount { get; init; }
        public decimal? BaseAmount { get; init; }
        public decimal? Percentage { get; init; }
        public string? Reason { get; init; }
        public string? ReasonCode { get; init; }
    }
}
