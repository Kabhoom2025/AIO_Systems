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
using NovaERP.Infrastructure.Services;
using Xunit;

namespace NovaERP.Tests.Services;

public class MultiChannelNotificationTests
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

    private static IHttpClientFactory CreateHttpClientFactory()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();
        return services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
    }

    [Fact]
    public async Task EmailSender_Returns_NotConfigured_When_Settings_Are_Blank()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        ctx.Organizations.Add(org);
        ctx.NotificationChannelSettings.Add(new NotificationChannelSettings { OrganizationId = org.Id });
        await ctx.SaveChangesAsync();

        var sender = new SmtpEmailSender(ctx);
        var result = await sender.SendAsync(org.Id, "someone@example.com", "Subject", "Body");

        Assert.False(result.Success);
        Assert.Equal("NotConfigured", result.Status);
    }

    [Fact]
    public async Task SmsSender_Returns_NotConfigured_When_Settings_Are_Blank()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        ctx.Organizations.Add(org);
        await ctx.SaveChangesAsync();

        var sender = new HttpSmsSender(ctx, CreateHttpClientFactory());
        var result = await sender.SendAsync(org.Id, "+911234567890", "Hello");

        Assert.False(result.Success);
        Assert.Equal("NotConfigured", result.Status);
    }

    [Fact]
    public async Task PushSender_Returns_NotConfigured_When_Settings_Are_Blank()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        ctx.Organizations.Add(org);
        await ctx.SaveChangesAsync();

        var sender = new HttpPushSender(ctx, CreateHttpClientFactory());
        var result = await sender.SendAsync(org.Id, "device-token", "Title", "Hello");

        Assert.False(result.Success);
        Assert.Equal("NotConfigured", result.Status);
    }

    [Fact]
    public async Task NotificationChannelSettingsService_CreatesRow_Then_Updates_InPlace()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        ctx.Organizations.Add(org);
        await ctx.SaveChangesAsync();

        var service = new NotificationChannelSettingsService(
            new NotificationChannelSettingsRepository(ctx), CreateMapper(), new UpdateNotificationChannelSettingsDtoValidator());

        var empty = await service.GetAsync(org.Id);
        Assert.Null(empty.SmtpHost);

        var updated = await service.UpdateAsync(org.Id, new UpdateNotificationChannelSettingsDto
        {
            SmtpHost = "smtp.example.com",
            SmtpFromEmail = "no-reply@example.com",
            SmtpUseSsl = true
        });

        Assert.Equal("smtp.example.com", updated.SmtpHost);
        Assert.Single(await ctx.NotificationChannelSettings.ToListAsync());

        var updatedAgain = await service.UpdateAsync(org.Id, new UpdateNotificationChannelSettingsDto
        {
            SmtpHost = "smtp2.example.com",
            SmtpFromEmail = "no-reply@example.com"
        });

        Assert.Equal("smtp2.example.com", updatedAgain.SmtpHost);
        Assert.Single(await ctx.NotificationChannelSettings.ToListAsync()); // still one row, updated not duplicated
    }

    [Fact]
    public async Task MultiChannelNotificationDispatcher_Never_Throws_And_Logs_NotConfigured_Channels()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        var role = new Role { Name = "Employee" };
        ctx.Organizations.Add(org);
        ctx.Roles.Add(role);
        await ctx.SaveChangesAsync();

        var user = new User { OrganizationId = org.Id, Name = "User", Email = "user@acme.test", RoleId = role.Id, IsActive = true };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var notificationService = new NotificationService(ctx, CreateMapper(), new CreateNotificationDtoValidator());
        var httpFactory = CreateHttpClientFactory();
        var dispatcher = new NovaERP.Infrastructure.Services.MultiChannelNotificationDispatcher(
            ctx, notificationService,
            new SmtpEmailSender(ctx), new HttpSmsSender(ctx, httpFactory), new HttpPushSender(ctx, httpFactory));

        await dispatcher.DispatchAsync(org.Id, user.Id, "Title", "Message",
            NovaERP.Application.Common.NotificationChannels.InApp
            | NovaERP.Application.Common.NotificationChannels.Email
            | NovaERP.Application.Common.NotificationChannels.Sms
            | NovaERP.Application.Common.NotificationChannels.Push);

        Assert.Single(await ctx.Notifications.ToListAsync());
        var logs = await ctx.NotificationDeliveryLogs.ToListAsync();
        Assert.Equal(3, logs.Count); // Email, Sms, Push each logged
        Assert.All(logs, l => Assert.Equal("NotConfigured", l.Status));
    }
}
