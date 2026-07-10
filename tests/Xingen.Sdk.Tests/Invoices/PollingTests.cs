using System.Net;
using Xingen.Sdk.Errors;
using Xingen.Sdk.Http;
using Xingen.Sdk.Invoices;
using Xingen.Sdk.Models;
using Xunit;

namespace Xingen.Sdk.Tests.Invoices;

/// <summary>
/// Exercises the <c>*AndWaitAsync</c> polling loop against a real loopback server. Backoff intervals
/// are configured in single-digit milliseconds so the tests run fast without needing to fake the
/// clock — <see cref="PollOptions"/> already makes every timing knob caller-configurable.
/// </summary>
public class PollingTests : IAsyncLifetime
{
    private static readonly PollOptions FastPoll = new()
    {
        InitialInterval = TimeSpan.FromMilliseconds(5),
        MaxInterval = TimeSpan.FromMilliseconds(20),
        BackoffMultiplier = 1.5,
        Timeout = TimeSpan.FromSeconds(5),
    };

    private LoopbackServer _server = null!;
    private InvoicesClient _client = null!;

    public Task InitializeAsync()
    {
        _server = new LoopbackServer().Start();
        var requestBuilder = new RequestBuilder(_server.BaseUrl, "xgn_test_abc123", "test-agent");
        _client = new InvoicesClient(new HttpClientTransport(new HttpClient()), requestBuilder, new JsonCodec());
        return Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _server.DisposeAsync();

    [Fact]
    public async Task SubmitAndWaitPollsUntilValidated()
    {
        _server.MapHandler("/v1/invoices", async context =>
        {
            if (context.Request.HttpMethod == "POST")
            {
                await context.RespondAsync(202, "{\"id\":\"inv_1\",\"status\":\"processing\"}");
            }
            else
            {
                context.Response.StatusCode = 405;
                context.Response.Close();
            }
        });
        var pollCount = 0;
        _server.MapHandler("/v1/invoices/inv_1", async context =>
        {
            var terminal = Interlocked.Increment(ref pollCount) >= 3;
            await context.RespondAsync(200, RecordJson("inv_1", terminal ? "validated" : "processing", true));
        });

        var result = await _client.SubmitAndWaitAsync(MinimalSubmission(), FastPoll);

        Assert.Equal(InvoiceStatus.Validated, result.Status);
        Assert.True(result.ValidationResult!.Valid);
    }

    [Fact]
    public async Task ValidateFileAndWaitReturnsNormallyOnFailedValidation()
    {
        _server.MapHandler("/v1/invoices/validate", context =>
            context.RespondAsync(202, "{\"id\":\"inv_2\",\"status\":\"processing\"}"));
        _server.MapHandler("/v1/invoices/inv_2", context =>
            context.RespondAsync(200, RecordJson("inv_2", "failed_validation", false)));

        var result = await _client.ValidateFileAndWaitAsync(
            "invoice.xml", "<x/>"u8.ToArray(), ValidationProfile.EN16931, FastPoll);

        Assert.Equal(InvoiceStatus.FailedValidation, result.Status);
        Assert.False(result.ValidationResult!.Valid);
    }

    [Fact]
    public async Task TimesOutWithPartialResultWhenDeadlineElapses()
    {
        _server.MapHandler("/v1/invoices", context => context.RespondAsync(202, "{\"id\":\"inv_3\",\"status\":\"processing\"}"));
        _server.MapHandler("/v1/invoices/inv_3", context => context.RespondAsync(200, RecordJson("inv_3", "processing", true)));

        var immediateTimeout = FastPoll with { Timeout = TimeSpan.Zero };

        var ex = await Assert.ThrowsAsync<XingenTimeoutException>(
            () => _client.SubmitAndWaitAsync(MinimalSubmission(), immediateTimeout));

        Assert.Equal("inv_3", ex.PartialResult.Id);
        Assert.Equal(InvoiceStatus.Processing, ex.PartialResult.Status);
    }

    [Fact]
    public async Task CancellationTokenAbortsPolling()
    {
        _server.MapHandler("/v1/invoices", context => context.RespondAsync(202, "{\"id\":\"inv_4\",\"status\":\"processing\"}"));
        _server.MapHandler("/v1/invoices/inv_4", context => context.RespondAsync(200, RecordJson("inv_4", "processing", true)));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(10));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _client.SubmitAndWaitAsync(MinimalSubmission(), FastPoll, cts.Token));
    }

    private static InvoiceSubmission MinimalSubmission() => new()
    {
        InvoiceNumber = "INV-1",
        IssueDate = new DateOnly(2026, 1, 1),
        Currency = "EUR",
        ValidationProfile = ValidationProfile.EN16931,
        Supplier = new InvoiceSubmission.PartyInput { Name = "Seller" },
        Buyer = new InvoiceSubmission.PartyInput { Name = "Buyer" },
        Lines =
        [
            new InvoiceSubmission.LineInput
            {
                Description = "Item", Quantity = 1m, Unit = "C62", Price = 10m, TaxRate = 0m,
            },
        ],
    };

    private static string RecordJson(string id, string status, bool valid)
    {
        var processing = status == "processing";
        var canonicalJson = processing ? "null" : "{\"invoiceNumber\":\"INV-1\",\"currency\":\"EUR\",\"lines\":[],\"notes\":[]}";
        var validationResult = processing ? "null" : $"{{\"valid\":{valid.ToString().ToLowerInvariant()},\"errors\":[],\"kositResult\":null}}";
        return $"{{\"id\":\"{id}\",\"status\":\"{status}\",\"createdAt\":\"2026-07-08T09:30:00Z\","
            + "\"validationProfile\":\"EN16931\",\"invoiceFormat\":\"UBL\",\"uploadedBy\":\"user_abc\","
            + "\"sandbox\":false,\"apiKeyId\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\","
            + $"\"canonicalJson\":{canonicalJson},\"validationResult\":{validationResult}}}";
    }
}
