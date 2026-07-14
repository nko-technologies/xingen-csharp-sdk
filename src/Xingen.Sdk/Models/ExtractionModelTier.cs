using System.Text.Json.Serialization;

namespace Xingen.Sdk.Models;

/// <summary>
/// Extraction quality/cost tier for AI-based PDF invoice extraction:
/// - <c>FAST</c> — lower-cost model, good for clean/text-based PDFs (available on every plan)
/// - <c>ACCURATE</c> — highest-accuracy model, recommended for scanned/low-quality PDFs (Pro only)
/// </summary>
// Uppercase to match the wire value exactly (Java's default enum JSON serialization).
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExtractionModelTier
{
    FAST,
    ACCURATE,
}
