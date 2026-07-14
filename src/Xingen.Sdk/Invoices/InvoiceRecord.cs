using System.Text.Json.Serialization;
using Xingen.Sdk.Models;

namespace Xingen.Sdk.Invoices;

/// <summary>
/// The envelope the backend returns for a submitted invoice: submission metadata plus the parsed
/// <see cref="Invoice"/> and, once validation has finished, its <see cref="ValidationResult"/>.
/// Distinct from <c>Invoice</c> itself because <c>GET /v1/invoices/{id}</c> never returns a bare
/// invoice — the backend column backing this field is named <c>canonicalJson</c>, remapped here to
/// <c>Invoice</c> so the SDK's public property name is meaningful rather than leaking an internal
/// storage detail.
/// </summary>
public sealed record InvoiceRecord
{
    public string? Id { get; init; }
    public InvoiceStatus Status { get; init; }

    [JsonPropertyName("canonicalJson")]
    public Invoice? Invoice { get; init; }

    /// <summary>Null while <see cref="Status"/> is <see cref="InvoiceStatus.Processing"/>.</summary>
    public ValidationResult? ValidationResult { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    public string? ValidationProfile { get; init; }
    public string? InvoiceFormat { get; init; }
    public string? UploadedBy { get; init; }
    public bool Sandbox { get; init; }
    public Guid? ApiKeyId { get; init; }

    /// <summary>
    /// Extraction quality tier used (<c>"FAST"</c>/<c>"ACCURATE"</c>) — only set for AI PDF
    /// extractions (<see cref="InvoiceFormat"/> <c>== "PDF_AI"</c>). Plain nullable string, not a
    /// typed enum, to match how sibling fields like <see cref="InvoiceFormat"/> and
    /// <see cref="ValidationProfile"/> are already typed on this record.
    /// </summary>
    public string? ExtractionTier { get; init; }
}
