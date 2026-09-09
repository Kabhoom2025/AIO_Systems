using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.Features.WorkItems;
using ProjectFlowAI.Application.Mapping;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Unit;

public class CreateWorkItemCommandHandlerTests
{
    private static IMapper CreateMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

    private static async Task<(CreateWorkItemCommandHandler Handler, Infrastructure.Data.ProjectFlowDbContext Db, Project Project, User Reporter)> CreateHandlerAsync()
    {
        var db = InMemoryDbContextFactory.Create();
        var org = new Organization { Name = "Org", Slug = "org" };
        db.Organizations.Add(org);
        var reporter = new User { Email = "reporter@example.com", PasswordHash = "x", FirstName = "R", LastName = "L" };
        db.Users.Add(reporter);
        await db.SaveChangesAsync();

        var project = new Project { OrganizationId = org.Id, Key = "PFA", Name = "Test Project", OwnerUserId = reporter.Id };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var handler = new CreateWorkItemCommandHandler(db, CreateMapper(), new TestDoubles.NoOpNotificationDispatcher(), new TestDoubles.NoOpWorkflowEngine());
        return (handler, db, project, reporter);
    }

    [Fact]
    public async Task Create_With_Valid_Data_Creates_WorkItem_In_Backlog_With_Positive_Position()
    {
        var (handler, db, project, reporter) = await CreateHandlerAsync();

        var command = new CreateWorkItemCommand(project.Id, null, "Implement login", "desc",
            WorkItemPriority.High, WorkItemType.Story, 5, 8m, null, reporter.Id, null, false, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Implement login", result.Title);
        Assert.Equal(WorkItemStatus.Backlog, result.Status);
        Assert.True(result.Position > 0);
        Assert.Single(db.WorkItems);
        Assert.Contains(db.WorkItemActivities, a => a.WorkItemId == result.Id && a.Action == "Created");
    }

    [Fact]
    public async Task Create_Second_Item_Gets_A_Larger_Sparse_Position_Than_The_First()
    {
        var (handler, db, project, reporter) = await CreateHandlerAsync();

        var first = await handler.Handle(new CreateWorkItemCommand(project.Id, null, "First", null,
            WorkItemPriority.Medium, WorkItemType.Task, null, null, null, reporter.Id, null, false, null), CancellationToken.None);
        var second = await handler.Handle(new CreateWorkItemCommand(project.Id, null, "Second", null,
            WorkItemPriority.Medium, WorkItemType.Task, null, null, null, reporter.Id, null, false, null), CancellationToken.None);

        Assert.True(second.Position > first.Position);
    }

    [Fact]
    public async Task Create_With_Unknown_Project_Throws_NotFoundException()
    {
        var (handler, _, _, reporter) = await CreateHandlerAsync();

        var command = new CreateWorkItemCommand(Guid.NewGuid(), null, "Orphan", null,
            WorkItemPriority.Medium, WorkItemType.Task, null, null, null, reporter.Id, null, false, null);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
