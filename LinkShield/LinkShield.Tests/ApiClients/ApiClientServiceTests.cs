using LinkShield.Application.DTOs.ApiClients;
using LinkShield.Infrastructure.ApiKeyAuth;
using LinkShield.Infrastructure.Persistence;
using LinkShield.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LinkShield.Tests.ApiClients;

public class ApiClientServiceTests
{
    private static LinkShieldDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LinkShieldDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LinkShieldDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_UsesDefaultsWhenNotSpecified()
    {
        var ctx = CreateContext();
        var service = new ApiClientService(ctx);

        var client = await service.CreateAsync(new CreateApiClientRequest("Acme Corp", "ops@acme.com", null, null));

        Assert.Equal(1000, client.DailyQuota);
        Assert.Equal(60, client.RequestsPerMinute);
    }

    [Fact]
    public async Task CreateKeyAsync_ReturnsRawKeyOnce_AndOnlyStoresHash()
    {
        var ctx = CreateContext();
        var service = new ApiClientService(ctx);
        var client = await service.CreateAsync(new CreateApiClientRequest("Acme Corp", "ops@acme.com", null, null));

        var created = await service.CreateKeyAsync(client.Id, new CreateApiKeyRequest("CI key", null));

        Assert.StartsWith("ls_live_", created.RawKey);
        var storedKey = await ctx.ApiKeys.SingleAsync();
        Assert.Equal(ApiKeyHasher.Hash(created.RawKey), storedKey.KeyHash);
        Assert.NotEqual(created.RawKey, storedKey.KeyHash);
    }

    [Fact]
    public async Task RevokeKeyAsync_MarksKeyInactive()
    {
        var ctx = CreateContext();
        var service = new ApiClientService(ctx);
        var client = await service.CreateAsync(new CreateApiClientRequest("Acme Corp", "ops@acme.com", null, null));
        var created = await service.CreateKeyAsync(client.Id, new CreateApiKeyRequest("CI key", null));

        var revoked = await service.RevokeKeyAsync(created.Id);

        Assert.True(revoked);
        var storedKey = await ctx.ApiKeys.SingleAsync();
        Assert.False(storedKey.IsActive);
    }

    [Fact]
    public async Task ApiKeyValidator_ValidatesRawKeyAndRejectsRevoked()
    {
        var ctx = CreateContext();
        var clientService = new ApiClientService(ctx);
        var client = await clientService.CreateAsync(new CreateApiClientRequest("Acme Corp", "ops@acme.com", null, null));
        var created = await clientService.CreateKeyAsync(client.Id, new CreateApiKeyRequest("CI key", null));

        var validator = new ApiKeyValidator(ctx, new InMemoryApiKeyRateLimiter());
        var validResult = await validator.ValidateAndRecordUsageAsync(created.RawKey);
        Assert.True(validResult.IsValid);
        Assert.Equal(client.Id, validResult.ApiClientId);

        await clientService.RevokeKeyAsync(created.Id);
        var revokedResult = await validator.ValidateAndRecordUsageAsync(created.RawKey);
        Assert.False(revokedResult.IsValid);
    }

    [Fact]
    public async Task ApiKeyValidator_RejectsUnknownKey()
    {
        var ctx = CreateContext();
        var validator = new ApiKeyValidator(ctx, new InMemoryApiKeyRateLimiter());

        var result = await validator.ValidateAndRecordUsageAsync("ls_live_not-a-real-key");

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ApiKeyValidator_EnforcesPerMinuteRateLimit()
    {
        var ctx = CreateContext();
        var clientService = new ApiClientService(ctx);
        var client = await clientService.CreateAsync(new CreateApiClientRequest("Acme Corp", "ops@acme.com", 1000, 2));
        var created = await clientService.CreateKeyAsync(client.Id, new CreateApiKeyRequest("CI key", null));

        var validator = new ApiKeyValidator(ctx, new InMemoryApiKeyRateLimiter());

        Assert.True((await validator.ValidateAndRecordUsageAsync(created.RawKey)).IsValid);
        Assert.True((await validator.ValidateAndRecordUsageAsync(created.RawKey)).IsValid);

        var throttled = await validator.ValidateAndRecordUsageAsync(created.RawKey);
        Assert.False(throttled.IsValid);
        Assert.Contains("rate limit", throttled.DenyReason, StringComparison.OrdinalIgnoreCase);
    }
}

public class InMemoryApiKeyRateLimiterTests
{
    [Fact]
    public void TryConsume_AllowsUpToLimit_ThenRejects()
    {
        var limiter = new InMemoryApiKeyRateLimiter();
        var clientId = Guid.NewGuid();

        Assert.True(limiter.TryConsume(clientId, 3));
        Assert.True(limiter.TryConsume(clientId, 3));
        Assert.True(limiter.TryConsume(clientId, 3));
        Assert.False(limiter.TryConsume(clientId, 3));
    }

    [Fact]
    public void TryConsume_TracksDifferentClientsIndependently()
    {
        var limiter = new InMemoryApiKeyRateLimiter();
        var clientA = Guid.NewGuid();
        var clientB = Guid.NewGuid();

        Assert.True(limiter.TryConsume(clientA, 1));
        Assert.False(limiter.TryConsume(clientA, 1));
        Assert.True(limiter.TryConsume(clientB, 1));
    }
}
