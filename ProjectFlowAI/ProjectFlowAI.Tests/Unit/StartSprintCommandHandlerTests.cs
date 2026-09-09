using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.Features.Sprints;
using ProjectFlowAI.Application.Mapping;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Unit;

public class StartSprintCommandHandlerTests
{
    private static IMapper CreateMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

    private static async Task<(Infrastructure.Data.ProjectFlowDbContext Db, Project Project)> CreateProjectAsync()
    {
        var db = InMemoryDbContextFactory.Create();
        var org = new Organization { Name = "Org", Slug = "org" };
        db.Organizations.Add(org);
        var owner = new User { Email = "owner@example.com", PasswordHash = "x", FirstName = "O", LastName = "L" };
        db.Users.Add(owner);
        await db.SaveChangesAsync();

        var project = new Project { OrganizationId = org.Id, Key = "PFA", Name = "Test Project", OwnerUserId = owner.Id };
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return (db, project);
    }

    [Fact]
    public async Task Start_A_Planned_Sprint_Sets_Status_To_Active()
    {
        var (db, project) = await CreateProjectAsync();
        var sprint = new Sprint { ProjectId = project.Id, Name = "Sprint 1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(14) };
        db.Sprints.Add(sprint);
        await db.SaveChangesAsync();

        var handler = new StartSprintCommandHandler(db, CreateMapper(), new NoOpNotificationDispatcher(), new NoOpWorkflowEngine());
        var result = await handler.Handle(new StartSprintCommand(sprint.Id), CancellationToken.None);

        Assert.Equal(SprintStatus.Active, result.Status);
    }

    [Fact]
    public async Task Starting_A_Sprint_When_Another_Sprint_In_The_Same_Project_Is_Already_Active_Throws_Conflict()
    {
        var (db, project) = await CreateProjectAsync();
        var activeSprint = new Sprint { ProjectId = project.Id, Name = "Active Sprint", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(14), Status = SprintStatus.Active };
        var plannedSprint = new Sprint { ProjectId = project.Id, Name = "Next Sprint", StartDate = DateTime.UtcNow.AddDays(14), EndDate = DateTime.UtcNow.AddDays(28) };
        db.Sprints.AddRange(activeSprint, plannedSprint);
        await db.SaveChangesAsync();

        var handler = new StartSprintCommandHandler(db, CreateMapper(), new NoOpNotificationDispatcher(), new NoOpWorkflowEngine());

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new StartSprintCommand(plannedSprint.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Completing_A_Sprint_Returns_NonDone_Items_To_The_Backlog_But_Keeps_Done_Items_Linked()
    {
        var (db, project) = await CreateProjectAsync();
        var owner = await db.Users.FirstAsync();
        var sprint = new Sprint { ProjectId = project.Id, Name = "Sprint 1", StartDate = DateTime.UtcNow.AddDays(-7), EndDate = DateTime.UtcNow, Status = SprintStatus.Active };
        db.Sprints.Add(sprint);
        await db.SaveChangesAsync();

        var doneItem = new WorkItem { ProjectId = project.Id, Title = "Done item", Status = WorkItemStatus.Done, ReporterUserId = owner.Id, SprintId = sprint.Id, StoryPoints = 3 };
        var inProgressItem = new WorkItem { ProjectId = project.Id, Title = "Still going", Status = WorkItemStatus.InProgress, ReporterUserId = owner.Id, SprintId = sprint.Id, StoryPoints = 5 };
        db.WorkItems.AddRange(doneItem, inProgressItem);
        await db.SaveChangesAsync();

        var handler = new CompleteSprintCommandHandler(db, CreateMapper());
        await handler.Handle(new CompleteSprintCommand(sprint.Id), CancellationToken.None);

        var reloadedDone = await db.WorkItems.FirstAsync(w => w.Id == doneItem.Id);
        var reloadedInProgress = await db.WorkItems.FirstAsync(w => w.Id == inProgressItem.Id);

        Assert.Equal(sprint.Id, reloadedDone.SprintId);
        Assert.Null(reloadedInProgress.SprintId);
    }
}
