using FlowSphere.Application.Common;
using FlowSphere.Application.Workflows.Commands.ExecuteWorkflow;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Tests.TestUtilities;

namespace FlowSphere.Tests.Application.Workflows;

public class ExecuteWorkflowCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext db, WorkflowDefinition workflow)>
        SeedPublishedWorkflowAsync(string dbName, FakeCurrentUserContext currentUser, int monthlyExecutionQuota)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);

        var org = new Organization { Id = currentUser.OrganizationId, Name = "Acme Corp", MonthlyExecutionQuota = monthlyExecutionQuota };
        db.Organizations.Add(org);

        var workflow = WorkflowDefinition.Create(currentUser.OrganizationId, "Test Workflow", null, currentUser.UserId);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        var version = new WorkflowVersion
        {
            WorkflowDefinitionId = workflow.Id,
            VersionNumber = 1,
            Status = VersionStatus.Published,
            PublishedAt = DateTime.UtcNow,
        };
        db.WorkflowVersions.Add(version);
        await db.SaveChangesAsync();

        return (db, workflow);
    }

    [Fact]
    public async Task Handle_UnderQuota_EnqueuesExecution()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 11 };
        var (db, workflow) = await SeedPublishedWorkflowAsync(nameof(Handle_UnderQuota_EnqueuesExecution), currentUser, monthlyExecutionQuota: 5);
        await using var _ = db;
        var queue = new FakeExecutionQueue();

        var handler = new ExecuteWorkflowCommandHandler(db, currentUser, queue);
        var result = await handler.Handle(new ExecuteWorkflowCommand(workflow.Id, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(queue.Enqueued);
        Assert.Equal(result.Value, queue.Enqueued[0]);
    }

    [Fact]
    public async Task Handle_AtQuota_ReturnsQuotaExceededAndDoesNotEnqueue()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 11 };
        var (db, workflow) = await SeedPublishedWorkflowAsync(nameof(Handle_AtQuota_ReturnsQuotaExceededAndDoesNotEnqueue), currentUser, monthlyExecutionQuota: 1);
        await using var _ = db;
        var queue = new FakeExecutionQueue();
        var handler = new ExecuteWorkflowCommandHandler(db, currentUser, queue);

        var first = await handler.Handle(new ExecuteWorkflowCommand(workflow.Id, null), CancellationToken.None);
        Assert.True(first.IsSuccess);

        var second = await handler.Handle(new ExecuteWorkflowCommand(workflow.Id, null), CancellationToken.None);

        Assert.False(second.IsSuccess);
        Assert.Equal(ErrorType.QuotaExceeded, second.Error!.Type);
        Assert.Single(queue.Enqueued); // only the first execution made it onto the queue
    }

    [Fact]
    public async Task Handle_NoPublishedVersion_ReturnsNotFound()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 11 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_NoPublishedVersion_ReturnsNotFound));
        db.Organizations.Add(new Organization { Id = currentUser.OrganizationId, Name = "Acme Corp" });
        var workflow = WorkflowDefinition.Create(currentUser.OrganizationId, "Draft Only", null, currentUser.UserId);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        var handler = new ExecuteWorkflowCommandHandler(db, currentUser, new FakeExecutionQueue());
        var result = await handler.Handle(new ExecuteWorkflowCommand(workflow.Id, null), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
