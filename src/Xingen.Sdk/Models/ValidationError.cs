namespace Xingen.Sdk.Models;

public sealed record ValidationError
{
    public string? Code { get; init; }
    public string? Message { get; init; }
    public string? Field { get; init; }
    public string? Suggestion { get; init; }
    public string? DocumentationUrl { get; init; }
    public ValidationLayer Layer { get; init; }
    public Severity Severity { get; init; }
}
