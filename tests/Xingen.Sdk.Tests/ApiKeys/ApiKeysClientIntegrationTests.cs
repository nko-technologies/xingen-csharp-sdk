using Xingen.Sdk;
using Xingen.Sdk.ApiKeys;
using Xingen.Sdk.Errors;
using Xunit;

namespace Xingen.Sdk.Tests.ApiKeys;

/// <summary>Exercises <see cref="ApiKeysClient"/> against a real (loopback) HTTP server so the actual <see cref="HttpClient"/> code path is under test.</summary>
public class ApiKeysClientIntegrationTests : IAsyncLifetime
{
    private LoopbackServer _server = null!;
    private XingenClient _client = null!;

    public Task InitializeAsync()
    {
        _server = new LoopbackServer().Start();
        _client = new XingenClient("xgn_test_abc123", _server.BaseUrl);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _server.DisposeAsync();
    }

    [Fact]
    public async Task CreateReturnsRawKeyOnce()
    {
        var id = Guid.NewGuid();
        string? capturedAuth = null;
        string? capturedBody = null;
        _server.MapHandler("/v1/api-keys", async context =>
        {
            if (context.Request.HttpMethod != "POST")
            {
                context.Response.StatusCode = 405;
                context.Response.Close();
                return;
            }
            capturedAuth = context.Request.Headers["Authorization"];
            capturedBody = await context.ReadBodyAsync();
            var body = $"{{\"id\":\"{id}\",\"rawKey\":\"xgn_test_generated\",\"name\":\"CI\","
                + "\"sandbox\":true,\"quotaLimit\":null,\"createdAt\":\"2026-07-08T00:00:00Z\"}";
            await context.RespondAsync(201, body);
        });

        var created = await _client.ApiKeys.CreateAsync(new CreateApiKeyRequest { Name = "CI", Sandbox = true });

        Assert.Equal(id, created.Id);
        Assert.Equal("xgn_test_generated", created.RawKey);
        Assert.True(created.Sandbox);
        Assert.Null(created.QuotaLimit);

        Assert.Equal("Bearer xgn_test_abc123", capturedAuth);
        Assert.Contains("\"name\":\"CI\"", capturedBody);
        Assert.Contains("\"sandbox\":true", capturedBody);
    }

    [Fact]
    public async Task ListDeserializesEachKey()
    {
        var id = Guid.NewGuid();
        _server.MapHandler("/v1/api-keys", context =>
        {
            var body = $"[{{\"id\":\"{id}\",\"name\":\"CI\",\"keyPrefix\":\"xgn_live\",\"sandbox\":false,"
                + "\"active\":true,\"quotaLimit\":10000,\"quotaUsed\":42,\"lastUsedAt\":null,"
                + "\"createdAt\":\"2026-07-01T00:00:00Z\",\"revokedAt\":null}]";
            return context.RespondAsync(200, body);
        });

        var keys = await _client.ApiKeys.ListAsync();

        var key = Assert.Single(keys);
        Assert.Equal(id, key.Id);
        Assert.Equal(42, key.QuotaUsed);
        Assert.True(key.Active);
    }

    [Fact]
    public async Task RevokeSendsDeleteToKeyPath()
    {
        var id = Guid.NewGuid();
        string? capturedMethod = null;
        _server.MapHandler($"/v1/api-keys/{id}", context =>
        {
            capturedMethod = context.Request.HttpMethod;
            context.Response.StatusCode = 204;
            context.Response.Close();
            return Task.CompletedTask;
        });

        await _client.ApiKeys.RevokeAsync(id);

        Assert.Equal("DELETE", capturedMethod);
    }

    [Fact]
    public async Task RevokeUnknownKeyThrowsNotFound()
    {
        var id = Guid.NewGuid();
        _server.MapHandler($"/v1/api-keys/{id}", context => context.RespondAsync(404,
            "{\"message\":\"API key not found\",\"error\":\"NOT_FOUND\",\"code\":404,\"timestamp\":\"2026-07-08T00:00:00Z\"}"));

        await Assert.ThrowsAsync<NotFoundException>(() => _client.ApiKeys.RevokeAsync(id));
    }
}
