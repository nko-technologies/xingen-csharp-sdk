namespace Xingen.Sdk.Paging;

/// <summary>Mirrors the JSON shape Spring Data's <c>Page&lt;T&gt;</c> serializes to.</summary>
public sealed record Page<T>
{
    public List<T> Content { get; init; } = [];
    public long TotalElements { get; init; }
    public int TotalPages { get; init; }
    public int Number { get; init; }
    public int Size { get; init; }
    public bool First { get; init; }
    public bool Last { get; init; }
    public int NumberOfElements { get; init; }
    public bool Empty { get; init; }
}
