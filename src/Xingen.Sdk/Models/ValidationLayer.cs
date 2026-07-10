using System.Text.Json.Serialization;

namespace Xingen.Sdk.Models;

// Uppercase to match the wire value exactly (Java's default enum JSON serialization).
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValidationLayer
{
    /// <summary>EN16931</summary>
    CORE,

    /// <summary>XRechnung</summary>
    NATIONAL,

    /// <summary>Peppol</summary>
    NETWORK,
}
