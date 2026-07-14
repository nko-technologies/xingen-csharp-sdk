namespace Xingen.Sdk.Models;

/// <summary>A field the backend fills in automatically when it isn't supplied, e.g. from <see cref="Invoices.InvoicesClient.GetAutoFilledFieldsAsync"/>.</summary>
public sealed record AutoFilledField
{
    /// <summary>Canonical Invoice field path, e.g. "typeCode" or "lines[].lineId".</summary>
    public string? Field { get; init; }

    /// <summary>The value that will be set, or a short description when it isn't a fixed value.</summary>
    public string? Value { get; init; }

    /// <summary>Why it's set automatically, in user-facing language.</summary>
    public string? Reason { get; init; }
}
