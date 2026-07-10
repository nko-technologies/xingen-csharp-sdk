namespace Xingen.Sdk.ApiKeys;

/// <summary>Request body for <see cref="ApiKeysClient.CreateAsync"/>.</summary>
public sealed record CreateApiKeyRequest
{
    public required string Name { get; init; }

    /// <summary>If true, requests using this key don't count toward quota. Defaults to false.</summary>
    public bool Sandbox { get; init; }

    /// <summary>Optional monthly quota. Null means unlimited (Pro only) — free-tier keys are capped server-side regardless.</summary>
    public int? QuotaLimit { get; init; }
}
