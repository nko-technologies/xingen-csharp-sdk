using System.Text.Json.Serialization;

namespace Xingen.Sdk.Models;

/// <summary>
/// - <c>EN16931</c> — European standard EN 16931 (free)
/// - <c>PEPPOL</c> — PEPPOL BIS Billing 3.0 (free)
/// - <c>XRECHNUNG</c> — German XRechnung standard (Pro)
/// - <c>FRANCE</c> — French Factur-X standard (Pro) — not yet implemented server-side
/// - <c>ITALY</c> — Italian FatturaPA standard (Pro) — not yet implemented server-side
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValidationProfile
{
    EN16931,
    PEPPOL,
    XRECHNUNG,
    FRANCE,
    ITALY,
}
