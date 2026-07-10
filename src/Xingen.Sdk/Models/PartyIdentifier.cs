namespace Xingen.Sdk.Models;

public sealed record PartyIdentifier
{
    public string? Id { get; init; }

    /// <summary>ISO 6523 ICD, or "SEPA" for creditor identifiers.</summary>
    public string? SchemeId { get; init; }
}
