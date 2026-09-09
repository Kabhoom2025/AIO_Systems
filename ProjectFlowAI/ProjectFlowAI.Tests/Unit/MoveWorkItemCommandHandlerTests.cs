using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectFlowAI.Application.Features.WorkItems;
using ProjectFlowAI.Application.Mapping;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Unit;

public class MoveWorkItemCommandHandlerTests
{
    private static IMapper CreateMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

    private static async Task<(MoveWorkItemCommandHandler Handler, Infrastructure.Data.ProjectFlowDbContext Db, Project Project, User User)> CreateHandlerAsync()
    {
        var db = InMemoryDbContextFactory.Create();
        var org = new Organization { Name = "Org", Slug = "org" };
        db.Organizations.Add(org);
        var user = new User { Email = "actor@example.com", PasswordHash = "x", FirstName = "A", LastName = "L" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = new Project { OrganizationId = org.Id, Key = "PFA", Name = "Test Project", OwnerUserId = user.Id };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var handler = new MoveWorkItemCommandHandler(db, CreateMapper(), new NoOpWorkflowEngine());
        return (handler, db, project, user);
    }

    [Fact]
    public async Task Move_To_Different_Status_Updates_Status_Position_And_Records_Activity()
    {
        var (handler, db, project, user) = await CreateHandlerAsync();
        var workItem = new WorkItem
        {
            ProjectId = project.Id, Title = "Task 1", Status = WorkItemStatus.Backlog,
            ReporterUserId = user.Id, Position = 1024
        };
        db.WorkItems.Add(workItem);
        await db.SaveChangesAsync();

        var result = await handler.Handle(new MoveWorkItemCommand(workItem.Id, WorkItemStatus.InProgress, 2048, user.Id), CancellationToken.None);

        Assert.Equal(WorkItemStatus.InProgress, result.Status);
        Assert.Equal(2048, result.Position);
        Assert.Contains(db.WorkItemActivities, a => a.WorkItemId == workItem.Id && a.Action == "StatusChanged");
    }

    [Fact]
    public async Task Moving_A_Recurring_WorkItem_To_Done_Creates_The_Next_Occurrence_With_Advanced_DueDate()
    {
        var (handler, db, project, user) = await CreateHandlerAsync();
        var dueDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var workItem = new WorkItem
        {
            ProjectId = project.Id, Title = "Weekly report", Status = WorkItemStatus.InProgress,
            ReporterUserId = user.Id, Position = 1024,
            IsRecurring = true, RecurrenceIntervalDays = 7, DueDate = dueDate
        };
        db.WorkItems.Add(workItem);
        await db.SaveChangesAsync();

        await handler.Handle(new MoveWorkItemCommand(workItem.Id, WorkItemStatus.Done, 4096, user.Id), CancellationToken.None);

        var allItems = await db.WorkItems.Where(w => w.ProjectId == project.Id).ToListAsync();
        Assert.Equal(2, allItems.Count);

        var nextOccurrence = allItems.Single(w => w.Id != workItem.Id);
        Assert.Equal(WorkItemStatus.Backlog, nextOccurrence.Status);
        Assert.True(nextOccurrence.IsRecurring);
        Assert.Equal(dueDate.AddDays(7), nextOccurrence.DueDate);
        Assert.Equal("Weekly report", nextOccurrence.Title);
    }

    [Fact]
    public async Task Moving_A_NonRecurring_WorkItem_To_Done_Does_Not_Create_A_New_Occurrence()
    {
        var (handler, db, project, user) = await CreateHandlerAsync();
        var workItem = new WorkItem
        {
            ProjectId = project.Id, Title = "One-off task", Status = WorkItemStatus.InProgress,
            ReporterUserId = user.Id, Position = 1024, IsRecurring = false
        };
        db.WorkItems.Add(workItem);
        await db.SaveChangesAsync();

        await handler.Handle(new MoveWorkItemCommand(workItem.Id, WorkItemStatus.Done, 4096, user.Id), CancellationToken.None);

        var count = await db.WorkItems.CountAsync(w => w.ProjectId == project.Id);
        Assert.Equal(1, count);
    }
}
