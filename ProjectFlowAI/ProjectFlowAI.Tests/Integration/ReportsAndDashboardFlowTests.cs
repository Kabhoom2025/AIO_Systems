using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectFlowAI.Application.Features.Dashboard;
using ProjectFlowAI.Application.Features.Reports;
using ProjectFlowAI.Application.Features.WorkItems;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Infrastructure.Data;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Integration;

/// <summary>Phase 5 smoke tests: seed a real org/project/work-items/sprint through the same
/// mediator-direct-through-DI pattern as SprintFlowTests/WorkItemFlowTests (auth is JWT-based, so
/// hitting the real HTTP pipeline isn't practical here), then confirm GetDashboardQuery and
/// GetExecutiveDashboardQuery return a well-formed body with no exception. Not asserting exact
/// counts — just that the shape is right and every aggregation runs to completion.</summary>
public class ReportsAndDashboardFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ReportsAndDashboardFlowTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IEmailSender>();
                services.AddScoped<IEmailSender, CapturingEmailSender>();
            });
        });

    [Fact]
    public async Task GetDashboard_For_A_Seeded_User_Returns_A_WellFormed_Body()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProjectFlowDbContext>();
        await db.Database.EnsureCreatedAsync();
        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        var org = new Organization { Name = $"Org-{Guid.NewGuid():N}", Slug = $"org-{Guid.NewGuid():N}" };
        db.Organizations.Add(org);
        var user = new User { Email = $"dash-{Guid.NewGuid():N}@example.com", PasswordHash = "x", FirstName = "Dash", LastName = "Board", OrganizationId = org.Id };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = await mediator.Send(new Application.Features.Projects.CreateProjectCommand(
            org.Id, "DSH", "Dashboard Test Project", "desc", user.Id, null, null));
        db.ProjectMembers.Add(new ProjectMember { ProjectId = project.Id, UserId = user.Id, RoleInProject = "Lead" });
        await db.SaveChangesAsync();

        var dueToday = await mediator.Send(new CreateWorkItemCommand(
            project.Id, null, "Due today", null, WorkItemPriority.High, WorkItemType.Task,
            3, null, user.Id, user.Id, DateTime.UtcNow.Date, false, null));
        var overdue = await mediator.Send(new CreateWorkItemCommand(
            project.Id, null, "Overdue item", null, WorkItemPriority.Medium, WorkItemType.Task,
            2, null, user.Id, user.Id, DateTime.UtcNow.Date.AddDays(-3), false, null));

        var result = await mediator.Send(new GetDashboardQuery(user.Id));

        Assert.NotNull(result);
        Assert.Contains(result.TodaysTasks, t => t.Id == dueToday.Id);
        Assert.Contains(result.OverdueTasks, t => t.Id == overdue.Id);
        Assert.True(result.AssignedTaskCount >= 2);
        Assert.NotNull(result.RecentActivity);
        Assert.NotNull(result.ActiveSprints);
        Assert.True(result.UnreadNotificationCount >= 0);
    }

    [Fact]
    public async Task GetExecutiveDashboard_For_A_Seeded_Org_Returns_A_WellFormed_Body()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProjectFlowDbContext>();
        await db.Database.EnsureCreatedAsync();
        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        var org = new Organization { Name = $"Org-{Guid.NewGuid():N}", Slug = $"org-{Guid.NewGuid():N}" };
        db.Organizations.Add(org);
        var user = new User { Email = $"exec-{Guid.NewGuid():N}@example.com", PasswordHash = "x", FirstName = "Exec", LastName = "User", OrganizationId = org.Id };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = await mediator.Send(new Application.Features.Projects.CreateProjectCommand(
            org.Id, "EXC", "Executive Test Project", "desc", user.Id, null, null));

        await mediator.Send(new CreateWorkItemCommand(
            project.Id, null, "Some overdue work", null, WorkItemPriority.High, WorkItemType.Task,
            5, null, user.Id, user.Id, DateTime.UtcNow.Date.AddDays(-1), false, null));

        var result = await mediator.Send(new GetExecutiveDashboardQuery(org.Id));

        Assert.NotNull(result);
        Assert.True(result.TotalProjects >= 1);
        Assert.True(result.TotalWorkItems >= 1);
        Assert.Contains(result.ProjectHealthSummaries, s => s.ProjectId == project.Id);
    }
}
