namespace Xingen.Sdk.Models;

public sealed record TaxBreakdown
{
    public decimal? TaxableAmount { get; init; }
    public decimal? TaxAmount { get; init; }

    /// <summary>S / Z / E / AE / K / G / O</summary>
    public string? CategoryCode { get; init; }

    /// <summary>Null for exempt categories (E/AE/K/G/O).</summary>
    public decimal? Rate { get; init; }

    public string? ExemptionReason { get; init; }
    public string? ExemptionReasonCode { get; init; }
}
