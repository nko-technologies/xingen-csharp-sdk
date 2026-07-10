namespace Xingen.Sdk.Models;

/// <summary>Document-level allowance/charge (BG-20/BG-21).</summary>
public sealed record AllowanceCharge
{
    /// <summary>true = charge, false = allowance.</summary>
    public bool Charge { get; init; }

    public decimal? Amount { get; init; }
    public decimal? BaseAmount { get; init; }
    public decimal? Percentage { get; init; }
    public string? VatCategoryCode { get; init; }
    public decimal? VatRate { get; init; }
    public string? Reason { get; init; }
    public string? ReasonCode { get; init; }
}
