using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectFlowAI.Application.Features.Sprints;
using ProjectFlowAI.Application.Features.WorkItems;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Infrastructure.Data;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Integration;

/// <summary>End-to-end Scrum lifecycle: create sprint -> start it -> add a work item to it ->
/// move it to Done -> complete the sprint -> verify any remaining (non-Done) items return to the
/// backlog while the Done item stays linked. Mirrors WorkItemFlowTests' WebApplicationFactory + direct-mediator pattern.</summary>
public class SprintFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SprintFlowTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IEmailSender>();
                services.AddScoped<IEmailSender, CapturingEmailSender>();
            });
        });

    private async Task<(Organization Org, User Owner)> SeedOrgAndOwnerAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProjectFlowDbContext>();
        await db.Database.EnsureCreatedAsync();

        var org = new Organization { Name = $"Org-{Guid.NewGuid():N}", Slug = $"org-{Guid.NewGuid():N}" };
        db.Organizations.Add(org);
        var owner = new User { Email = $"owner-{Guid.NewGuid():N}@example.com", PasswordHash = "x", FirstName = "Own", LastName = "Er", OrganizationId = org.Id };
        db.Users.Add(owner);
        await db.SaveChangesAsync();
        return (org, owner);
    }

    [Fact]
    public async Task CreateSprint_Start_AssignWorkItem_MoveToDone_Complete_Returns_Remaining_Items_To_Backlog()
    {
        var (org, owner) = await SeedOrgAndOwnerAsync();

        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        var project = await mediator.Send(new Application.Features.Projects.CreateProjectCommand(
            org.Id, "SPR", "Sprint Test Project", "desc", owner.Id, null, null));

        var sprint = await mediator.Send(new CreateSprintCommand(
            project.Id, "Sprint 1", "Ship it", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(14)));
        Assert.Equal(SprintStatus.Planned, sprint.Status);

        var started = await mediator.Send(new StartSprintCommand(sprint.Id));
        Assert.Equal(SprintStatus.Active, started.Status);

        var itemToFinish = await mediator.Send(new CreateWorkItemCommand(
            project.Id, null, "Finish this one", null, WorkItemPriority.High, WorkItemType.Task,
            5, null, null, owner.Id, null, false, null));
        var itemLeftBehind = await mediator.Send(new CreateWorkItemCommand(
            project.Id, null, "Still in progress", null, WorkItemPriority.Medium, WorkItemType.Task,
            3, null, null, owner.Id, null, false, null));

        await mediator.Send(new MoveWorkItemToSprintCommand(itemToFinish.Id, sprint.Id));
        await mediator.Send(new MoveWorkItemToSprintCommand(itemLeftBehind.Id, sprint.Id));

        var board = await mediator.Send(new GetSprintBoardQuery(sprint.Id));
        Assert.Equal(2, board.Columns.Sum(c => c.Items.Count));

        await mediator.Send(new MoveWorkItemCommand(itemToFinish.Id, WorkItemStatus.Done, 4096, owner.Id));

        var completed = await mediator.Send(new CompleteSprintCommand(sprint.Id));
        Assert.Equal(SprintStatus.Completed, completed.Status);

        var db = scope.ServiceProvider.GetRequiredService<ProjectFlowDbContext>();
        var finishedItem = await db.WorkItems.FindAsync(itemToFinish.Id);
        var leftBehindItem = await db.WorkItems.FindAsync(itemLeftBehind.Id);

        Assert.Equal(sprint.Id, finishedItem!.SprintId);
        Assert.Null(leftBehindItem!.SprintId);
    }
}
