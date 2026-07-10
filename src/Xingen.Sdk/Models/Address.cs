namespace Xingen.Sdk.Models;

public sealed record Address
{
    public string? StreetName { get; init; }
    public string? AdditionalStreetName { get; init; }
    public string? AddressLine3 { get; init; }
    public string? City { get; init; }
    public string? PostalZone { get; init; }
    public string? CountrySubdivision { get; init; }
    public string? CountryCode { get; init; }
}
