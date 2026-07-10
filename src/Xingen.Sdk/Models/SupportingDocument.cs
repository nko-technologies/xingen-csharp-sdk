namespace Xingen.Sdk.Models;

public sealed record SupportingDocument
{
    public string? Id { get; init; }
    public string? SchemeId { get; init; }

    /// <summary>UNTDID 1001 document type code, e.g. "50", "130".</summary>
    public string? TypeCode { get; init; }

    public string? Description { get; init; }

    /// <summary>Null = no external-reference element; empty string = present but the URI is missing.</summary>
    public string? ExternalUri { get; init; }

    public string? MimeCode { get; init; }
    public string? Filename { get; init; }
    public bool EmbeddedPresent { get; init; }
}
