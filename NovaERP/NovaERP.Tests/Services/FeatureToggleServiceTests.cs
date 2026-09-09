using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaERP.Application.DTOs;
using NovaERP.Application.Mapping;
using NovaERP.Application.Services;
using NovaERP.Application.Validators;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;
using NovaERP.Infrastructure.Repositories;
using Xunit;

namespace NovaERP.Tests.Services;

public class FeatureToggleServiceTests
{
    private static NovaErpDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NovaErpDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NovaErpDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    private static FeatureToggleService CreateService(NovaErpDbContext ctx) =>
        new(new FeatureToggleRepository(ctx), CreateMapper(), new UpdateFeatureToggleDtoValidator());

    [Fact]
    public async Task UpdateAsync_Creates_Toggle_Row_When_None_Exists()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        ctx.Organizations.Add(org);
        await ctx.SaveChangesAsync();

        var result = await CreateService(ctx).UpdateAsync(org.Id, "crm",
            new UpdateFeatureToggleDto { ModuleKey = "crm", IsEnabled = true });

        Assert.Equal("crm", result.ModuleKey);
        Assert.True(result.IsEnabled);
        Assert.Single(await ctx.FeatureToggles.ToListAsync());
    }

    [Fact]
    public async Task UpdateAsync_Flips_Existing_Row_Rather_Than_Duplicating()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        ctx.Organizations.Add(org);
        ctx.FeatureToggles.Add(new FeatureToggle { OrganizationId = org.Id, ModuleKey = "sales", IsEnabled = false });
        await ctx.SaveChangesAsync();

        await CreateService(ctx).UpdateAsync(org.Id, "sales",
            new UpdateFeatureToggleDto { ModuleKey = "sales", IsEnabled = true });

        var toggles = await ctx.FeatureToggles
            .Where(f => f.OrganizationId == org.Id && f.ModuleKey == "sales")
            .ToListAsync();
        Assert.Single(toggles);
        Assert.True(toggles[0].IsEnabled);
    }
}
