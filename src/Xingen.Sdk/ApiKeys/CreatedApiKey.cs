namespace Xingen.Sdk.ApiKeys;

/// <summary>
/// Response from <see cref="ApiKeysClient.CreateAsync"/>. <see cref="RawKey"/> is shown only this
/// once — the backend never returns it again, so callers must persist it immediately.
/// </summary>
public sealed record CreatedApiKey
{
    public Guid Id { get; init; }
    public string? RawKey { get; init; }
    public string? Name { get; init; }
    public bool Sandbox { get; init; }

    /// <summary>Null means unlimited.</summary>
    public int? QuotaLimit { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Redacts <see cref="RawKey"/> so it doesn't end up in logs via an unguarded ToString/log call.</summary>
    public override string ToString() =>
        $"CreatedApiKey {{ Id = {Id}, RawKey = ***, Name = {Name}, Sandbox = {Sandbox}, QuotaLimit = {QuotaLimit}, CreatedAt = {CreatedAt} }}";
}
