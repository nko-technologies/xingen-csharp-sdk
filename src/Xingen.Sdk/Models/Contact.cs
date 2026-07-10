namespace Xingen.Sdk.Models;

public sealed record Contact
{
    public string? Name { get; init; }
    public string? Telephone { get; init; }
    public string? Email { get; init; }
}
