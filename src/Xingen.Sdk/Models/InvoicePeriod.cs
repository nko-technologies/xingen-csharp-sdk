namespace Xingen.Sdk.Models;

/// <summary>Invoicing period, at document level (BG-14) or line level (BG-26).</summary>
public sealed record InvoicePeriod
{
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    /// <summary>Document level only (UNTDID 2005 tax point date code).</summary>
    public string? DescriptionCode { get; init; }
}
