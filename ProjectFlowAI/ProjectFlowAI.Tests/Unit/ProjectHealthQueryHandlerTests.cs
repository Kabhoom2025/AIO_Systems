using ProjectFlowAI.Application.Features.Reports;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Unit;

/// <summary>Covers the Green/Yellow/Red boundaries of the shared ProjectHealthScorer (exercised
/// here through the public GetProjectHealthQueryHandler, since the scorer itself is an internal
/// implementation detail shared with GetExecutiveDashboardQueryHandler): Red if overdueCount > 5 OR
/// any item has been Blocked for more than 3 days; Yellow if overdueCount is 1-5; Green otherwise.</summary>
public class ProjectHealthQueryHandlerTests
{
    private static async Task<(Infrastructure.Data.ProjectFlowDbContext Db, Project Project, User User)> CreateProjectAsync()
    {
        var db = InMemoryDbContextFactory.Create();
        var org = new Organization { Name = "Org", Slug = "org" };
        db.Organizations.Add(org);
        var user = new User { Email = "u@example.com", PasswordHash = "x", FirstName = "U", LastName = "L" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = new Project { OrganizationId = org.Id, Key = "PFA", Name = "Test Project", OwnerUserId = user.Id };
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return (db, project, user);
    }

    [Fact]
    public async Task No_Overdue_And_No_Long_Blocked_Items_Is_Green()
    {
        var (db, project, user) = await CreateProjectAsync();
        db.WorkItems.Add(new WorkItem { ProjectId = project.Id, Title = "On track", ReporterUserId = user.Id, Status = WorkItemStatus.InProgress });
        await db.SaveChangesAsync();

        var handler = new GetProjectHealthQueryHandler(db);
        var result = await handler.Handle(new GetProjectHealthQuery(project.Id), CancellationToken.None);

        Assert.Equal(ProjectHealthStatus.Green, result.Status);
        Assert.Empty(result.Reasons);
    }

    [Fact]
    public async Task One_To_Five_Overdue_Items_Is_Yellow()
    {
        var (db, project, user) = await CreateProjectAsync();
        for (var i = 0; i < 3; i++)
            db.WorkItems.Add(new WorkItem
            {
                ProjectId = project.Id, Title = $"Overdue {i}", ReporterUserId = user.Id,
                Status = WorkItemStatus.ToDo, DueDate = DateTime.UtcNow.Date.AddDays(-2)
            });
        await db.SaveChangesAsync();

        var handler = new GetProjectHealthQueryHandler(db);
        var result = await handler.Handle(new GetProjectHealthQuery(project.Id), CancellationToken.None);

        Assert.Equal(ProjectHealthStatus.Yellow, result.Status);
        Assert.Equal(3, result.OverdueCount);
    }

    [Fact]
    public async Task More_Than_Five_Overdue_Items_Is_Red()
    {
        var (db, project, user) = await CreateProjectAsync();
        for (var i = 0; i < 6; i++)
            db.WorkItems.Add(new WorkItem
            {
                ProjectId = project.Id, Title = $"Overdue {i}", ReporterUserId = user.Id,
                Status = WorkItemStatus.ToDo, DueDate = DateTime.UtcNow.Date.AddDays(-2)
            });
        await db.SaveChangesAsync();

        var handler = new GetProjectHealthQueryHandler(db);
        var result = await handler.Handle(new GetProjectHealthQuery(project.Id), CancellationToken.None);

        Assert.Equal(ProjectHealthStatus.Red, result.Status);
        Assert.Equal(6, result.OverdueCount);
    }

    [Fact]
    public async Task Item_Blocked_For_More_Than_Three_Days_Is_Red_Even_With_No_Overdue_Items()
    {
        var (db, project, user) = await CreateProjectAsync();
        var blockedItem = new WorkItem { ProjectId = project.Id, Title = "Blocked long", ReporterUserId = user.Id, Status = WorkItemStatus.Blocked };
        db.WorkItems.Add(blockedItem);
        await db.SaveChangesAsync();

        db.WorkItemActivities.Add(new WorkItemActivity
        {
            WorkItemId = blockedItem.Id, UserId = user.Id, Action = "StatusChanged",
            FieldName = "Status", OldValue = "InProgress", NewValue = "Blocked",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        });
        await db.SaveChangesAsync();

        var handler = new GetProjectHealthQueryHandler(db);
        var result = await handler.Handle(new GetProjectHealthQuery(project.Id), CancellationToken.None);

        Assert.Equal(ProjectHealthStatus.Red, result.Status);
        Assert.Equal(1, result.BlockedCount);
        Assert.Contains(result.Reasons, r => r.Contains("blocked for 5 day"));
    }

    [Fact]
    public async Task Item_Blocked_For_Three_Days_Or_Fewer_Does_Not_Trigger_Red()
    {
        var (db, project, user) = await CreateProjectAsync();
        var blockedItem = new WorkItem { ProjectId = project.Id, Title = "Blocked briefly", ReporterUserId = user.Id, Status = WorkItemStatus.Blocked };
        db.WorkItems.Add(blockedItem);
        await db.SaveChangesAsync();

        db.WorkItemActivities.Add(new WorkItemActivity
        {
            WorkItemId = blockedItem.Id, UserId = user.Id, Action = "StatusChanged",
            FieldName = "Status", OldValue = "InProgress", NewValue = "Blocked",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        });
        await db.SaveChangesAsync();

        var handler = new GetProjectHealthQueryHandler(db);
        var result = await handler.Handle(new GetProjectHealthQuery(project.Id), CancellationToken.None);

        Assert.Equal(ProjectHealthStatus.Green, result.Status);
    }
}
