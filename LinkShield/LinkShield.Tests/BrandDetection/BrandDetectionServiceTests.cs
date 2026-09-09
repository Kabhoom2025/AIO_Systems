using LinkShield.Domain.Entities;
using LinkShield.Infrastructure.Persistence;
using LinkShield.Infrastructure.Risk;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LinkShield.Tests.BrandDetection;

public class BrandDetectionServiceTests
{
    private static async Task<LinkShieldDbContext> CreateSeededContextAsync()
    {
        var options = new DbContextOptionsBuilder<LinkShieldDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new LinkShieldDbContext(options);
        ctx.BrandProfiles.AddRange(
            new BrandProfile { BrandName = "PayPal", OfficialDomain = "paypal.com" },
            new BrandProfile { BrandName = "Microsoft", OfficialDomain = "microsoft.com" });
        await ctx.SaveChangesAsync();
        return ctx;
    }

    [Fact]
    public async Task DetectAsync_BrandNameEmbeddedInHyphenatedDomain_IsFlagged()
    {
        var ctx = await CreateSeededContextAsync();
        var service = new BrandDetectionService(ctx);

        var matches = await service.DetectAsync("paypal-secure-login.tk", "http://paypal-secure-login.tk/account/verify");

        Assert.Contains(matches, m => m.BrandName == "PayPal" && m.MatchType == "BrandNameInDomain");
    }

    [Fact]
    public async Task DetectAsync_TyposquattedTokenInHyphenatedDomain_IsFlagged()
    {
        var ctx = await CreateSeededContextAsync();
        var service = new BrandDetectionService(ctx);

        var matches = await service.DetectAsync("paypa1-secure-login.tk", "http://paypa1-secure-login.tk/account/verify");

        Assert.Contains(matches, m => m.BrandName == "PayPal" && m.MatchType == "Typosquat");
    }

    [Fact]
    public async Task DetectAsync_OfficialDomain_ProducesNoMatch()
    {
        var ctx = await CreateSeededContextAsync();
        var service = new BrandDetectionService(ctx);

        var matches = await service.DetectAsync("paypal.com", "https://paypal.com/login");

        Assert.Empty(matches);
    }

    [Fact]
    public async Task DetectAsync_UnrelatedDomain_ProducesNoMatch()
    {
        var ctx = await CreateSeededContextAsync();
        var service = new BrandDetectionService(ctx);

        var matches = await service.DetectAsync("example.com", "https://example.com/about");

        Assert.Empty(matches);
    }
}
