using LinkShield.Application.DTOs.Admin;
using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.Persistence;
using LinkShield.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LinkShield.Tests.Admin;

public class AdminServicesTests
{
    private static LinkShieldDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LinkShieldDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LinkShieldDbContext(options);
    }

    [Fact]
    public async Task BrandManagementService_CreateAsync_AddsBrandAndAuditLog()
    {
        var ctx = CreateContext();
        var service = new BrandManagementService(ctx);
        var actingUserId = Guid.NewGuid();

        var result = await service.CreateAsync(new CreateBrandProfileRequest("Netflix", "netflix.com", null), actingUserId);

        Assert.Equal("Netflix", result.BrandName);
        Assert.Single(await ctx.BrandProfiles.ToListAsync());
        Assert.Contains(await ctx.AuditLogs.ToListAsync(), a => a.Action == AuditAction.BrandProfileChanged && a.UserId == actingUserId);
    }

    [Fact]
    public async Task BrandManagementService_CreateAsync_RejectsDuplicateOfficialDomain()
    {
        var ctx = CreateContext();
        var service = new BrandManagementService(ctx);
        await service.CreateAsync(new CreateBrandProfileRequest("Netflix", "netflix.com", null), null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateBrandProfileRequest("Netflix Clone", "netflix.com", null), null));
    }

    [Fact]
    public async Task RiskRuleManagementService_UpdateAsync_ChangesWeightAndDisables()
    {
        var ctx = CreateContext();
        var service = new RiskRuleManagementService(ctx);
        var created = await service.CreateAsync(
            new CreateRiskRuleRequest("TestRule", "desc", RiskFactorCategory.UrlAnalysis, 0.10m, "Test.Condition", 5), null);

        var updated = await service.UpdateAsync(
            created.Id, new UpdateRiskRuleRequest("TestRule", "desc", 0.25m, false, "Test.Condition", 8), null);

        Assert.Equal(0.25m, updated.Weight);
        Assert.False(updated.IsEnabled);
        Assert.Equal(8, updated.ScoreContribution);
    }

    [Fact]
    public async Task RiskRuleManagementService_UpdateAsync_ThrowsForUnknownId()
    {
        var ctx = CreateContext();
        var service = new RiskRuleManagementService(ctx);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateAsync(Guid.NewGuid(), new UpdateRiskRuleRequest("X", "Y", 0.1m, true, "Z", 1), null));
    }

    [Fact]
    public async Task AuditLogService_GetAllAsync_ReturnsMostRecentFirst()
    {
        var ctx = CreateContext();
        ctx.AuditLogs.Add(new Domain.Entities.AuditLog { Action = AuditAction.Login, EntityType = "User", OccurredAtUtc = DateTime.UtcNow.AddMinutes(-5) });
        ctx.AuditLogs.Add(new Domain.Entities.AuditLog { Action = AuditAction.Register, EntityType = "User", OccurredAtUtc = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var service = new AuditLogService(ctx);
        var result = await service.GetAllAsync(1, 10);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(AuditAction.Register, result.Items[0].Action);
    }
}
