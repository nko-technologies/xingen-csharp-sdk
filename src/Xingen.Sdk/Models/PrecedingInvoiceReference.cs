namespace Xingen.Sdk.Models;

public sealed record PrecedingInvoiceReference
{
    public string? Id { get; init; }
    public DateOnly? IssueDate { get; init; }
}
