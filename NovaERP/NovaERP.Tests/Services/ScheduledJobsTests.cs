using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaERP.Application.Mapping;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;
using NovaERP.Infrastructure.Jobs;
using NovaERP.Infrastructure.Services;
using Xunit;

namespace NovaERP.Tests.Services;

public class ScheduledJobsTests
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

    private static NotificationService CreateNotificationService(NovaErpDbContext ctx) =>
        new(ctx, CreateMapper(), new NovaERP.Application.Validators.CreateNotificationDtoValidator());

    [Fact]
    public async Task ApprovalReminderJob_Notifies_Approver_For_Steps_Pending_Over_24_Hours()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        var managerRole = new Role { Name = "Manager" };
        ctx.Organizations.Add(org);
        ctx.Roles.Add(managerRole);
        await ctx.SaveChangesAsync();

        var manager = new User { OrganizationId = org.Id, Name = "Mgr", Email = "mgr@acme.test", RoleId = managerRole.Id, IsActive = true };
        var employee = new User { OrganizationId = org.Id, Name = "Emp", Email = "emp@acme.test", RoleId = managerRole.Id, IsActive = true };
        ctx.Users.AddRange(manager, employee);
        await ctx.SaveChangesAsync();

        var definition = new WorkflowDefinition { OrganizationId = org.Id, Name = "Expense", EntityType = "ExpenseClaim", IsActive = true };
        ctx.WorkflowDefinitions.Add(definition);
        await ctx.SaveChangesAsync();

        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = definition.Id,
            EntityType = "ExpenseClaim",
            EntityId = 1,
            Status = "Pending",
            CurrentStepOrder = 1,
            SubmittedByUserId = employee.Id,
            SubmittedDate = DateTime.UtcNow.AddHours(-30)
        };
        ctx.WorkflowInstances.Add(instance);
        await ctx.SaveChangesAsync();

        ctx.WorkflowStepInstances.Add(new WorkflowStepInstance
        {
            WorkflowInstanceId = instance.Id,
            StepOrder = 1,
            ApproverRoleId = managerRole.Id,
            Status = "Pending"
        });

        ctx.ScheduledJobDefinitions.Add(new ScheduledJobDefinition
        {
            OrganizationId = org.Id, Name = "Approval Reminder", JobKey = "ApprovalReminder", CronExpression = "0 9 * * *", IsEnabled = true
        });
        await ctx.SaveChangesAsync();

        var job = new ApprovalReminderJob(ctx, CreateNotificationService(ctx));
        await job.RunAsync();

        var notifications = await ctx.Notifications.Where(n => n.UserId == manager.Id).ToListAsync();
        Assert.Single(notifications);
        Assert.Contains("pending your approval", notifications[0].Message);

        var jobDef = await ctx.ScheduledJobDefinitions.FirstAsync(j => j.JobKey == "ApprovalReminder");
        Assert.Equal("Success", jobDef.LastRunStatus);
        Assert.NotNull(jobDef.LastRunAt);
    }

    [Fact]
    public async Task ApprovalReminderJob_Ignores_Steps_Pending_Under_24_Hours()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        var managerRole = new Role { Name = "Manager" };
        ctx.Organizations.Add(org);
        ctx.Roles.Add(managerRole);
        await ctx.SaveChangesAsync();

        var manager = new User { OrganizationId = org.Id, Name = "Mgr", Email = "mgr@acme.test", RoleId = managerRole.Id, IsActive = true };
        ctx.Users.Add(manager);
        await ctx.SaveChangesAsync();

        var definition = new WorkflowDefinition { OrganizationId = org.Id, Name = "Expense", EntityType = "ExpenseClaim", IsActive = true };
        ctx.WorkflowDefinitions.Add(definition);
        await ctx.SaveChangesAsync();

        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = definition.Id,
            EntityType = "ExpenseClaim",
            EntityId = 2,
            Status = "Pending",
            CurrentStepOrder = 1,
            SubmittedByUserId = manager.Id,
            SubmittedDate = DateTime.UtcNow.AddHours(-1)
        };
        ctx.WorkflowInstances.Add(instance);
        await ctx.SaveChangesAsync();

        ctx.WorkflowStepInstances.Add(new WorkflowStepInstance
        {
            WorkflowInstanceId = instance.Id, StepOrder = 1, ApproverRoleId = managerRole.Id, Status = "Pending"
        });
        await ctx.SaveChangesAsync();

        var job = new ApprovalReminderJob(ctx, CreateNotificationService(ctx));
        await job.RunAsync();

        Assert.Empty(await ctx.Notifications.ToListAsync());
    }

    [Fact]
    public async Task StaleExchangeRateCheckJob_Notifies_Admins_When_Rates_Are_Stale()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        var adminRole = new Role { Name = "Admin" };
        ctx.Organizations.Add(org);
        ctx.Roles.Add(adminRole);
        await ctx.SaveChangesAsync();

        var admin = new User { OrganizationId = org.Id, Name = "Admin", Email = "admin@acme.test", RoleId = adminRole.Id, IsActive = true };
        ctx.Users.Add(admin);
        ctx.ExchangeRates.Add(new ExchangeRate
        {
            OrganizationId = org.Id, FromCurrencyCode = "INR", ToCurrencyCode = "USD", Rate = 0.012m,
            EffectiveDate = DateTime.UtcNow.AddDays(-45)
        });
        ctx.ScheduledJobDefinitions.Add(new ScheduledJobDefinition
        {
            OrganizationId = org.Id, Name = "Stale Exchange Rate Check", JobKey = "StaleExchangeRateCheck", CronExpression = "0 8 * * 1", IsEnabled = true
        });
        await ctx.SaveChangesAsync();

        var job = new StaleExchangeRateCheckJob(ctx, CreateNotificationService(ctx));
        await job.RunAsync();

        var notifications = await ctx.Notifications.Where(n => n.UserId == admin.Id).ToListAsync();
        Assert.Single(notifications);
        Assert.Contains("exchange rates", notifications[0].Title, StringComparison.OrdinalIgnoreCase);

        var jobDef = await ctx.ScheduledJobDefinitions.FirstAsync(j => j.JobKey == "StaleExchangeRateCheck");
        Assert.Equal("Success", jobDef.LastRunStatus);
    }

    [Fact]
    public async Task StaleExchangeRateCheckJob_Skips_Orgs_With_Only_Fresh_Rates()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        var adminRole = new Role { Name = "Admin" };
        ctx.Organizations.Add(org);
        ctx.Roles.Add(adminRole);
        await ctx.SaveChangesAsync();

        var admin = new User { OrganizationId = org.Id, Name = "Admin", Email = "admin@acme.test", RoleId = adminRole.Id, IsActive = true };
        ctx.Users.Add(admin);
        ctx.ExchangeRates.Add(new ExchangeRate
        {
            OrganizationId = org.Id, FromCurrencyCode = "INR", ToCurrencyCode = "USD", Rate = 0.012m,
            EffectiveDate = DateTime.UtcNow.AddDays(-1)
        });
        await ctx.SaveChangesAsync();

        var job = new StaleExchangeRateCheckJob(ctx, CreateNotificationService(ctx));
        await job.RunAsync();

        Assert.Empty(await ctx.Notifications.ToListAsync());
    }
}
