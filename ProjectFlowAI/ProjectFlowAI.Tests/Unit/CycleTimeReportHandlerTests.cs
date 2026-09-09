using ProjectFlowAI.Application.Features.Reports;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Unit;

/// <summary>Covers the cycle-time/lead-time judgment call documented on
/// GetCycleTimeReportQueryHandler: an item with a full Backlog->InProgress->Done activity trail
/// gets both a lead time and a cycle time; an item with no recorded "work started" transition
/// (e.g. Backlog -> Done directly) is excluded from the cycle-time average but still counted in
/// the lead-time average.</summary>
public class CycleTimeReportHandlerTests
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
    public async Task Item_With_Full_Backlog_InProgress_Done_Trail_Computes_Correct_Lead_And_Cycle_Hours()
    {
        var (db, project, user) = await CreateProjectAsync();

        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var inProgressAt = createdAt.AddHours(2);
        var doneAt = createdAt.AddHours(10);

        var item = new WorkItem
        {
            ProjectId = project.Id, Title = "Full trail item", ReporterUserId = user.Id,
            Status = WorkItemStatus.Done, CreatedAt = createdAt
        };
        db.WorkItems.Add(item);
        await db.SaveChangesAsync();

        db.WorkItemActivities.AddRange(
            new WorkItemActivity { WorkItemId = item.Id, UserId = user.Id, Action = "Created", CreatedAt = createdAt },
            new WorkItemActivity { WorkItemId = item.Id, UserId = user.Id, Action = "StatusChanged", FieldName = "Status", OldValue = "ToDo", NewValue = "InProgress", CreatedAt = inProgressAt },
            new WorkItemActivity { WorkItemId = item.Id, UserId = user.Id, Action = "StatusChanged", FieldName = "Status", OldValue = "InProgress", NewValue = "Done", CreatedAt = doneAt });
        await db.SaveChangesAsync();

        var handler = new GetCycleTimeReportQueryHandler(db);
        var result = await handler.Handle(new GetCycleTimeReportQuery(project.Id, createdAt.AddDays(-1), doneAt.AddDays(1)), CancellationToken.None);

        var reported = Assert.Single(result.Items);
        Assert.Equal(10.0, reported.LeadTimeHours, 3);
        Assert.NotNull(reported.CycleTimeHours);
        Assert.Equal(8.0, reported.CycleTimeHours!.Value, 3);
        Assert.Equal(10.0, result.AverageLeadTimeHours!.Value, 3);
        Assert.Equal(8.0, result.AverageCycleTimeHours!.Value, 3);
    }

    [Fact]
    public async Task Item_With_No_InProgress_Transition_Is_Excluded_From_Cycle_Time_Average_But_Included_In_Lead_Time_Average()
    {
        var (db, project, user) = await CreateProjectAsync();

        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var doneAt = createdAt.AddHours(4);

        // Straight Backlog -> Done: only "Created" and the Done transition are recorded, no
        // intermediate non-Backlog/ToDo transition ever happened.
        var item = new WorkItem
        {
            ProjectId = project.Id, Title = "Straight-to-done item", ReporterUserId = user.Id,
            Status = WorkItemStatus.Done, CreatedAt = createdAt
        };
        db.WorkItems.Add(item);
        await db.SaveChangesAsync();

        db.WorkItemActivities.AddRange(
            new WorkItemActivity { WorkItemId = item.Id, UserId = user.Id, Action = "Created", CreatedAt = createdAt },
            new WorkItemActivity { WorkItemId = item.Id, UserId = user.Id, Action = "StatusChanged", FieldName = "Status", OldValue = "Backlog", NewValue = "Done", CreatedAt = doneAt });
        await db.SaveChangesAsync();

        var handler = new GetCycleTimeReportQueryHandler(db);
        var result = await handler.Handle(new GetCycleTimeReportQuery(project.Id, createdAt.AddDays(-1), doneAt.AddDays(1)), CancellationToken.None);

        var reported = Assert.Single(result.Items);
        Assert.Null(reported.CycleTimeHours);
        Assert.Equal(4.0, reported.LeadTimeHours, 3);
        // Only item in the report, and it has no cycle time -> average cycle time is null (no
        // items with a defined cycle time to average), while lead time average still reflects this item.
        Assert.Equal(4.0, result.AverageLeadTimeHours!.Value, 3);
        Assert.Null(result.AverageCycleTimeHours);
    }

    [Fact]
    public async Task Mixed_Set_Averages_Lead_Time_Over_Both_Items_But_Cycle_Time_Only_Over_The_One_With_A_Start_Transition()
    {
        var (db, project, user) = await CreateProjectAsync();

        var baseTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var withTrail = new WorkItem { ProjectId = project.Id, Title = "With trail", ReporterUserId = user.Id, Status = WorkItemStatus.Done, CreatedAt = baseTime };
        var withoutTrail = new WorkItem { ProjectId = project.Id, Title = "Without trail", ReporterUserId = user.Id, Status = WorkItemStatus.Done, CreatedAt = baseTime };
        db.WorkItems.AddRange(withTrail, withoutTrail);
        await db.SaveChangesAsync();

        var withTrailDoneAt = baseTime.AddHours(12);
        var withoutTrailDoneAt = baseTime.AddHours(6);

        db.WorkItemActivities.AddRange(
            new WorkItemActivity { WorkItemId = withTrail.Id, UserId = user.Id, Action = "StatusChanged", NewValue = "InProgress", CreatedAt = baseTime.AddHours(2) },
            new WorkItemActivity { WorkItemId = withTrail.Id, UserId = user.Id, Action = "StatusChanged", NewValue = "Done", CreatedAt = withTrailDoneAt },
            new WorkItemActivity { WorkItemId = withoutTrail.Id, UserId = user.Id, Action = "StatusChanged", NewValue = "Done", CreatedAt = withoutTrailDoneAt });
        await db.SaveChangesAsync();

        var handler = new GetCycleTimeReportQueryHandler(db);
        var result = await handler.Handle(new GetCycleTimeReportQuery(project.Id, baseTime.AddDays(-1), baseTime.AddDays(2)), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        // Lead time average = (12 + 6) / 2 = 9
        Assert.Equal(9.0, result.AverageLeadTimeHours!.Value, 3);
        // Cycle time average = only withTrail's 10 hours (12 - 2), not diluted by the other item.
        Assert.Equal(10.0, result.AverageCycleTimeHours!.Value, 3);
    }
}
