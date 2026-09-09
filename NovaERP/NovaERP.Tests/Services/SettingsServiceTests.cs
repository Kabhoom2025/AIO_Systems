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

public class SettingsServiceTests
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

    private static SettingsService CreateService(NovaErpDbContext ctx) =>
        new(new SettingsRepository(ctx), CreateMapper(), new UpdateOrganizationSettingsDtoValidator());

    [Fact]
    public async Task GetAsync_Creates_Default_Settings_Row_When_None_Exists()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        ctx.Organizations.Add(org);
        await ctx.SaveChangesAsync();

        var result = await CreateService(ctx).GetAsync(org.Id);

        Assert.Equal("en", result.DefaultLanguageCode);
        Assert.Equal("INR", result.DefaultCurrencyCode);
        Assert.Single(await ctx.OrganizationSettings.ToListAsync());
    }

    [Fact]
    public async Task GetAsync_Reuses_Existing_Row_Instead_Of_Creating_Another()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        ctx.Organizations.Add(org);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        await service.GetAsync(org.Id);
        await service.GetAsync(org.Id);

        Assert.Single(await ctx.OrganizationSettings.ToListAsync());
    }

    [Fact]
    public async Task UpdateAsync_Persists_Changes_To_The_Same_Row()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        ctx.Organizations.Add(org);
        await ctx.SaveChangesAsync();

        var updated = await CreateService(ctx).UpdateAsync(org.Id, new UpdateOrganizationSettingsDto
        {
            DefaultLanguageCode = "hi",
            DefaultCurrencyCode = "USD",
            DefaultTimezone = "Asia/Kolkata",
            DateFormat = "yyyy-MM-dd",
            TimeFormat = "HH:mm",
            FiscalYearStartMonth = 1,
            InvoiceNumberPrefix = "INV",
            TaxInclusivePricing = true
        });

        Assert.Equal("hi", updated.DefaultLanguageCode);
        Assert.Equal("USD", updated.DefaultCurrencyCode);
        Assert.True(updated.TaxInclusivePricing);
        Assert.Single(await ctx.OrganizationSettings.ToListAsync());
    }
}
