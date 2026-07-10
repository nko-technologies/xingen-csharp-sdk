namespace Xingen.Sdk.Models;

public sealed record ItemClassification
{
    public string? Code { get; init; }

    /// <summary>UNTDID 7143 scheme identifier.</summary>
    public string? ListId { get; init; }

    public string? ListVersionId { get; init; }
}
