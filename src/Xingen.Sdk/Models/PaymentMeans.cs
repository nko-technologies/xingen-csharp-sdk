namespace Xingen.Sdk.Models;

public sealed record PaymentMeans
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
