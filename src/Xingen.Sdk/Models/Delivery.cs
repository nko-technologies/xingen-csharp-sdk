namespace Xingen.Sdk.Models;

public sealed record Delivery
{
    public string? PartyName { get; init; }
    public string? LocationId { get; init; }
    public string? LocationSchemeId { get; init; }

    /// <summary>Deliver-to address (BG-15); null iff absent from the source document.</summary>
    public Address? Address { get; init; }

    public DateOnly? ActualDeliveryDate { get; init; }
}
