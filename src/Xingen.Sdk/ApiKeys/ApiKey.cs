namespace Xingen.Sdk.ApiKeys;

/// <summary>API key metadata as returned by list/create — the raw key value is never included here.</summary>
public sealed record ApiKey
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? KeyPrefix { get; init; }
    public bool Sandbox { get; init; }
    public bool Active { get; init; }

    /// <summary>Null means unlimited.</summary>
    public int? QuotaLimit { get; init; }

    public int QuotaUsed { get; init; }
    public DateTimeOffset? LastUsedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Null if the key is still active.</summary>
    public DateTimeOffset? RevokedAt { get; init; }
}
