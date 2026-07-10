namespace Xingen.Sdk.Models;

public sealed record KositResult
{
    public bool Valid { get; init; }
    public List<string> Errors { get; init; } = [];
}
