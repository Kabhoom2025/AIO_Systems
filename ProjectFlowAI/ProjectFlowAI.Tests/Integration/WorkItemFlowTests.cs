using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Infrastructure.Data;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Integration;

/// <summary>End-to-end: create-project -> create-work-item -> move-to-done -> verify the
/// activity log entry was actually written. Mirrors AuthFlowTests' WebApplicationFactory pattern.</summary>
public class WorkItemFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WorkItemFlowTests(WebApplicationFactory<Program> factory) =>
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

    private static HttpClient AuthorizedClient(WebApplicationFactory<Program> factory)
    {
        // These endpoints are [Authorize]-protected but this test only needs to reach the handlers'
        // business logic; ProjectFlowAI's JWT auth requires a real signed token, so instead we hit
        // MediatR handlers directly through the DI container for the parts that need auth context,
        // and use the HTTP client only for the parts that don't require it. See below.
        return factory.CreateClient();
    }

    [Fact]
    public async Task CreateProject_Then_CreateWorkItem_Then_MoveToDone_Writes_An_Activity_Log_Entry()
    {
        var (org, owner) = await SeedOrgAndOwnerAsync();

        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        var project = await mediator.Send(new Application.Features.Projects.CreateProjectCommand(
            org.Id, "FLW", "Flow Test Project", "desc", owner.Id, null, null));
        Assert.Equal("FLW", project.Key);

        var workItem = await mediator.Send(new Application.Features.WorkItems.CreateWorkItemCommand(
            project.Id, null, "Ship the feature", null, WorkItemPriority.High, WorkItemType.Task,
            null, null, owner.Id, owner.Id, null, false, null));
        Assert.Equal(WorkItemStatus.Backlog, workItem.Status);

        var moved = await mediator.Send(new Application.Features.WorkItems.MoveWorkItemCommand(
            workItem.Id, WorkItemStatus.Done, 4096, owner.Id));
        Assert.Equal(WorkItemStatus.Done, moved.Status);

        var db = scope.ServiceProvider.GetRequiredService<ProjectFlowDbContext>();
        var activities = db.WorkItemActivities.Where(a => a.WorkItemId == workItem.Id).ToList();
        Assert.Contains(activities, a => a.Action == "Created");
        Assert.Contains(activities, a => a.Action == "StatusChanged" && a.NewValue == "Done");

        var detail = await mediator.Send(new Application.Features.WorkItems.GetWorkItemQuery(workItem.Id));
        Assert.NotEmpty(detail.Activities);
    }

    [Fact]
    public async Task Health_Endpoint_Is_Reachable_Over_Http()
    {
        var client = AuthorizedClient(_factory);
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
