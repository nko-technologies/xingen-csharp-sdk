namespace Xingen.Sdk.Invoices;

/// <summary>
/// Configures the polling loop used by the <c>*AndWaitAsync</c> helpers on <see cref="InvoicesClient"/>.
/// Use a <c>with</c> expression off <see cref="Default"/> to override individual settings, e.g.
/// <c>PollOptions.Default with { Timeout = TimeSpan.FromMinutes(5) }</c>.
/// </summary>
public sealed record PollOptions
{
    public TimeSpan InitialInterval { get; init; } = TimeSpan.FromMilliseconds(500);

    public TimeSpan MaxInterval { get; init; } = TimeSpan.FromSeconds(5);

    public double BackoffMultiplier { get; init; } = 1.5;

    /// <summary>Total time budget across the whole poll loop, not a per-request timeout.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(60);

    public static PollOptions Default { get; } = new();
}
