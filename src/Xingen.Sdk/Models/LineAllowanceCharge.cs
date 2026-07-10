namespace Xingen.Sdk.Models;

/// <summary>Line-level allowance/charge (BT-136..BT-141).</summary>
public sealed record LineAllowanceCharge
{
    public bool Charge { get; init; }
    public decimal? Amount { get; init; }
    public decimal? BaseAmount { get; init; }
    public decimal? Percentage { get; init; }
    public string? Reason { get; init; }
    public string? ReasonCode { get; init; }
}
