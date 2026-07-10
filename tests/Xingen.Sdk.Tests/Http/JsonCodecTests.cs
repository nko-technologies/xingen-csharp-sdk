using System.Text;
using Xingen.Sdk.Http;
using Xingen.Sdk.Models;
using Xunit;

namespace Xingen.Sdk.Tests.Http;

public class JsonCodecTests
{
    private readonly JsonCodec _codec = new();

    /// <summary>
    /// Confirms System.Text.Json does NOT have the float64-intermediate precision-loss bug that hit
    /// the Python (pydantic) and TypeScript SDKs — those parsers tokenize JSON numbers into a
    /// float/double first, silently corrupting large/exact monetary literals before they ever reach
    /// the target numeric type. STJ instead parses UTF-8 number text directly into `decimal` when the
    /// target property is decimal-typed, so no intermediate float64 stage exists to lose precision.
    /// </summary>
    [Fact]
    public void DecodesHighPrecisionDecimalWithoutDoubleIntermediateLoss()
    {
        var json = Encoding.UTF8.GetBytes(
            "{\"taxableAmount\": 123456789012345.6789, \"taxAmount\": 0.1, \"categoryCode\": \"S\"}");

        var result = _codec.Decode<TaxBreakdown>(json);

        Assert.Equal(123456789012345.6789m, result.TaxableAmount);
        Assert.Equal(0.1m, result.TaxAmount);
    }

    [Fact]
    public void RoundTripsHighPrecisionDecimalOnEncode()
    {
        var value = new TaxBreakdown { TaxableAmount = 123456789012345.6789m, TaxAmount = 189.05m, CategoryCode = "S" };

        var encoded = _codec.Encode(value);
        var decoded = _codec.Decode<TaxBreakdown>(encoded);

        Assert.Equal(value.TaxableAmount, decoded.TaxableAmount);
    }

    [Fact]
    public void TryDecodeFieldReadsTopLevelStringWithoutCommittingToFullShape()
    {
        var json = Encoding.UTF8.GetBytes("{\"error\":\"Quota exceeded\"}");
        Assert.Equal("Quota exceeded", _codec.TryDecodeField(json, "error"));
    }

    [Fact]
    public void TryDecodeFieldReturnsNullForMissingOrNullField()
    {
        var json = Encoding.UTF8.GetBytes("{\"other\":\"x\",\"nullField\":null}");
        Assert.Null(_codec.TryDecodeField(json, "missing"));
        Assert.Null(_codec.TryDecodeField(json, "nullField"));
    }

    [Fact]
    public void TryDecodeReturnsNullOnMalformedBodyInsteadOfThrowing()
    {
        var json = Encoding.UTF8.GetBytes("not even json {{{");
        Assert.Null(_codec.TryDecode<TaxBreakdown>(json));
    }
}
