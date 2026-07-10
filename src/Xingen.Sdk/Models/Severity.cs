using System.Text.Json.Serialization;

namespace Xingen.Sdk.Models;

// Uppercase to match the wire value exactly (Java's default enum JSON serialization).
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Severity
{
    ERROR,
    WARNING,
    INFO,
}
