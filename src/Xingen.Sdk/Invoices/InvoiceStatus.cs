using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xingen.Sdk.Invoices;

[JsonConverter(typeof(InvoiceStatusJsonConverter))]
public enum InvoiceStatus
{
    Processing,
    Validated,
    FailedValidation,
}

public static class InvoiceStatusExtensions
{
    public static bool IsTerminal(this InvoiceStatus status) => status != InvoiceStatus.Processing;
}

internal sealed class InvoiceStatusJsonConverter : JsonConverter<InvoiceStatus>
{
    public override InvoiceStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "processing" => InvoiceStatus.Processing,
            "validated" => InvoiceStatus.Validated,
            "failed_validation" => InvoiceStatus.FailedValidation,
            _ => throw new JsonException($"Unknown invoice status: {value}"),
        };
    }

    public override void Write(Utf8JsonWriter writer, InvoiceStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            InvoiceStatus.Processing => "processing",
            InvoiceStatus.Validated => "validated",
            InvoiceStatus.FailedValidation => "failed_validation",
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        });
    }
}
