using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaERP.Application.Common;
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

public class WorkflowInstanceServiceTests
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

    private static WorkflowDefinitionService CreateDefinitionService(NovaErpDbContext ctx) =>
        new(new WorkflowDefinitionRepository(ctx), CreateMapper(),
            new CreateWorkflowDefinitionDtoValidator(), new UpdateWorkflowDefinitionDtoValidator());

    private static WorkflowInstanceService CreateInstanceService(NovaErpDbContext ctx)
    {
        var notificationService = new NotificationService(ctx, CreateMapper(), new CreateNotificationDtoValidator());
        var automationRuleRepo = new AutomationRuleRepository(ctx);
        var automationEngine = new AutomationEngine(ctx, automationRuleRepo, notificationService);
        return new WorkflowInstanceService(
            new WorkflowInstanceRepository(ctx), new WorkflowDefinitionRepository(ctx),
            new UserRepository(ctx), new AuditLogWriter(ctx), notificationService, automationEngine, CreateMapper());
    }

    private static AutomationRuleService CreateAutomationRuleService(NovaErpDbContext ctx) =>
        new(new AutomationRuleRepository(ctx), CreateMapper(),
            new CreateAutomationRuleDtoValidator(), new UpdateAutomationRuleDtoValidator());

    private record Fixture(Organization Org, Role ManagerRole, Role AdminRole, User Manager, User Admin, User Employee);

    private static async Task<Fixture> SeedFixtureAsync(NovaErpDbContext ctx)
    {
        var org = new Organization { Name = "Acme", Code = "ACME" };
        var managerRole = new Role { Name = "Manager" };
        var adminRole = new Role { Name = "Admin" };
        ctx.Organizations.Add(org);
        ctx.Roles.AddRange(managerRole, adminRole);
        await ctx.SaveChangesAsync();

        var manager = new User { OrganizationId = org.Id, Name = "Manager User", Email = "manager@acme.test", RoleId = managerRole.Id, IsActive = true };
        var admin = new User { OrganizationId = org.Id, Name = "Admin User", Email = "admin@acme.test", RoleId = adminRole.Id, IsActive = true };
        var employee = new User { OrganizationId = org.Id, Name = "Employee User", Email = "employee@acme.test", RoleId = managerRole.Id, IsActive = true };
        ctx.Users.AddRange(manager, admin, employee);
        await ctx.SaveChangesAsync();

        return new Fixture(org, managerRole, adminRole, manager, admin, employee);
    }

    private static async Task<WorkflowDefinitionDto> SeedTieredDefinitionAsync(NovaErpDbContext ctx, Fixture fx)
    {
        return await CreateDefinitionService(ctx).CreateAsync(fx.Org.Id, new CreateWorkflowDefinitionDto
        {
            Name = "Expense Claim Approval",
            EntityType = "ExpenseClaim",
            IsActive = true,
            Steps = new List<CreateWorkflowStepDefinitionDto>
            {
                new() { StepOrder = 1, Name = "Manager Approval", ApproverRoleId = fx.ManagerRole.Id, MinAmount = null },
                new() { StepOrder = 2, Name = "Admin Approval", ApproverRoleId = fx.AdminRole.Id, MinAmount = 10000m }
            }
        });
    }

    [Fact]
    public async Task CreateAsync_Persists_Definition_With_Ordered_Steps()
    {
        var ctx = CreateContext();
        var fx = await SeedFixtureAsync(ctx);

        var dto = await SeedTieredDefinitionAsync(ctx, fx);

        Assert.Equal(2, dto.Steps.Count);
        Assert.Equal(1, dto.Steps[0].StepOrder);
        Assert.Equal(2, dto.Steps[1].StepOrder);
        Assert.Single(await ctx.WorkflowDefinitions.ToListAsync());
    }

    [Fact]
    public async Task StartApproveApprove_HighValue_Claim_Completes_Both_Tiers()
    {
        var ctx = CreateContext();
        var fx = await SeedFixtureAsync(ctx);
        await SeedTieredDefinitionAsync(ctx, fx);

        var instanceService = CreateInstanceService(ctx);

        var started = await instanceService.StartAsync(fx.Org.Id, fx.Employee.Id, new StartWorkflowDto
        {
            EntityType = "ExpenseClaim",
            EntityId = 1,
            Amount = 15000m
        });

        Assert.Equal("Pending", started.Status);
        Assert.Equal(1, started.CurrentStepOrder);
        Assert.Equal(2, started.Steps.Count); // both tiers applicable at 15,000

        var afterManagerApproval = await instanceService.ApproveAsync(fx.Org.Id, started.Id, fx.Manager.Id,
            new WorkflowActionDto { Comments = "Looks fine" });

        Assert.Equal("Pending", afterManagerApproval.Status);
        Assert.Equal(2, afterManagerApproval.CurrentStepOrder);
        Assert.Null(afterManagerApproval.CompletedDate);

        var afterAdminApproval = await instanceService.ApproveAsync(fx.Org.Id, started.Id, fx.Admin.Id,
            new WorkflowActionDto { Comments = "Approved" });

        Assert.Equal("Approved", afterAdminApproval.Status);
        Assert.NotNull(afterAdminApproval.CompletedDate);
        Assert.All(afterAdminApproval.Steps, s => Assert.Equal("Approved", s.Status));
    }

    [Fact]
    public async Task LowValue_Claim_Only_Applies_The_Manager_Tier_And_Completes_On_First_Approval()
    {
        var ctx = CreateContext();
        var fx = await SeedFixtureAsync(ctx);
        await SeedTieredDefinitionAsync(ctx, fx);

        var instanceService = CreateInstanceService(ctx);

        var started = await instanceService.StartAsync(fx.Org.Id, fx.Employee.Id, new StartWorkflowDto
        {
            EntityType = "ExpenseClaim",
            EntityId = 2,
            Amount = 500m
        });

        Assert.Single(started.Steps); // only the manager tier applies below 10,000

        var result = await instanceService.ApproveAsync(fx.Org.Id, started.Id, fx.Manager.Id, new WorkflowActionDto());

        Assert.Equal("Approved", result.Status);
        Assert.NotNull(result.CompletedDate);
    }

    [Fact]
    public async Task RejectAsync_Marks_Instance_Rejected_And_Notifies_Submitter()
    {
        var ctx = CreateContext();
        var fx = await SeedFixtureAsync(ctx);
        await SeedTieredDefinitionAsync(ctx, fx);

        var instanceService = CreateInstanceService(ctx);
        var started = await instanceService.StartAsync(fx.Org.Id, fx.Employee.Id, new StartWorkflowDto
        {
            EntityType = "ExpenseClaim",
            EntityId = 3,
            Amount = 200m
        });

        var result = await instanceService.RejectAsync(fx.Org.Id, started.Id, fx.Manager.Id,
            new WorkflowActionDto { Comments = "Not a valid expense" });

        Assert.Equal("Rejected", result.Status);
        Assert.NotNull(result.CompletedDate);

        var submitterNotifications = await ctx.Notifications
            .Where(n => n.UserId == fx.Employee.Id)
            .ToListAsync();
        Assert.Contains(submitterNotifications, n => n.Title.Contains("rejected"));
    }

    [Fact]
    public async Task ApproveAsync_Throws_When_Acting_User_Does_Not_Hold_The_Required_Role()
    {
        var ctx = CreateContext();
        var fx = await SeedFixtureAsync(ctx);
        await SeedTieredDefinitionAsync(ctx, fx);

        var instanceService = CreateInstanceService(ctx);
        var started = await instanceService.StartAsync(fx.Org.Id, fx.Employee.Id, new StartWorkflowDto
        {
            EntityType = "ExpenseClaim",
            EntityId = 4,
            Amount = 200m
        });

        // Step 1 requires the Manager role; Admin does not hold it.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            instanceService.ApproveAsync(fx.Org.Id, started.Id, fx.Admin.Id, new WorkflowActionDto()));
    }

    [Fact]
    public async Task StartAsync_Throws_When_No_Workflow_Definition_Configured_For_Entity_Type()
    {
        var ctx = CreateContext();
        var fx = await SeedFixtureAsync(ctx);

        var instanceService = CreateInstanceService(ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            instanceService.StartAsync(fx.Org.Id, fx.Employee.Id, new StartWorkflowDto
            {
                EntityType = "PurchaseOrder",
                EntityId = 1,
                Amount = 100m
            }));
    }

    [Fact]
    public async Task AutomationRule_Fires_Notification_To_Target_Role_On_Full_Approval()
    {
        var ctx = CreateContext();
        var fx = await SeedFixtureAsync(ctx);
        await SeedTieredDefinitionAsync(ctx, fx);

        await CreateAutomationRuleService(ctx).CreateAsync(fx.Org.Id, new CreateAutomationRuleDto
        {
            Name = "Notify Admin on workflow approval",
            TriggerEvent = AutomationEvents.WorkflowApproved,
            ActionType = "Notify",
            NotifyRoleId = fx.AdminRole.Id,
            NotifyMessageTemplate = "Workflow '{EntityType}' #{EntityId} was approved.",
            IsEnabled = true
        });

        var instanceService = CreateInstanceService(ctx);
        var started = await instanceService.StartAsync(fx.Org.Id, fx.Employee.Id, new StartWorkflowDto
        {
            EntityType = "ExpenseClaim",
            EntityId = 9,
            Amount = 100m // low value: manager-only tier, completes on first approval
        });

        await instanceService.ApproveAsync(fx.Org.Id, started.Id, fx.Manager.Id, new WorkflowActionDto());

        var adminNotifications = await ctx.Notifications
            .Where(n => n.UserId == fx.Admin.Id && n.Message.Contains("ExpenseClaim"))
            .ToListAsync();
        Assert.Single(adminNotifications);
        Assert.Contains("#9", adminNotifications[0].Message);
    }
}
