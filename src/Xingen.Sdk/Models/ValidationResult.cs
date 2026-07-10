namespace Xingen.Sdk.Models;

public sealed record ValidationResult
{
    public bool Valid { get; init; }
    public List<ValidationError> Errors { get; init; } = [];

    /// <summary>Only populated for XML-based validation paths (UBL/CII/IDoc). Null otherwise.</summary>
    public KositResult? KositResult { get; init; }
}
