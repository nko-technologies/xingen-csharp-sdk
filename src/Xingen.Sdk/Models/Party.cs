namespace Xingen.Sdk.Models;

public sealed record Party
{
    public string? Name { get; init; }
    public string? RegistrationName { get; init; }
    public string? VatId { get; init; }
    public string? TaxRegistrationId { get; init; }
    public string? LegalRegistrationId { get; init; }
    public string? LegalRegistrationSchemeId { get; init; }
    public string? AdditionalLegalInfo { get; init; }
    public string? LeitwegId { get; init; }
    public string? EndpointId { get; init; }
    public string? EndpointSchemeId { get; init; }
    public List<PartyIdentifier> Identifiers { get; init; } = [];

    /// <summary>Null iff no postal address element was present in the source document.</summary>
    public Address? Address { get; init; }

    /// <summary>Null iff no contact element was present in the source document.</summary>
    public Contact? Contact { get; init; }
}
