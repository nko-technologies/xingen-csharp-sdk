using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xingen.Sdk.Http;

/// <summary>
/// Wraps a single, centrally-configured <see cref="JsonSerializerOptions"/>. Unknown properties are
/// tolerated on decode since the backend's response shapes evolve independently of SDK releases.
/// </summary>
public sealed class JsonCodec
{
    public JsonSerializerOptions Options { get; }

    public JsonCodec()
    {
        // Deliberately no global JsonStringEnumConverter here: a converter in this list takes
        // precedence over a type-level [JsonConverter] attribute, which would silently override
        // InvoiceStatus's custom snake_case converter. Each plain enum instead carries its own
        // [JsonConverter(typeof(JsonStringEnumConverter))] attribute.
        Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public byte[] Encode(object value) => JsonSerializer.SerializeToUtf8Bytes(value, value.GetType(), Options);

    public T Decode<T>(byte[] body) => JsonSerializer.Deserialize<T>(body, Options)!;

    /// <summary>Never throws — returns null on any parse failure, including an empty/null body.</summary>
    public T? TryDecode<T>(byte[]? body) where T : class
    {
        if (body is null || body.Length == 0)
        {
            return null;
        }
        try
        {
            return JsonSerializer.Deserialize<T>(body, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Reads a single top-level string field without committing to a full type — used for the 429 body shape.</summary>
    public string? TryDecodeField(byte[]? body, string fieldName)
    {
        if (body is null || body.Length == 0)
        {
            return null;
        }
        try
        {
            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty(fieldName, out var field) || field.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            return field.ValueKind == JsonValueKind.String ? field.GetString() : field.GetRawText();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    internal static string BodyAsString(byte[] body) => body.Length > 0 ? Encoding.UTF8.GetString(body) : "";
}
