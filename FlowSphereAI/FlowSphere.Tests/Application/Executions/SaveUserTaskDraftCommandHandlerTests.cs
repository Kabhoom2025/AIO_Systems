using FlowSphere.Application.Common;
using FlowSphere.Application.Executions.Commands.SaveUserTaskDraft;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Tests.TestUtilities;

namespace FlowSphere.Tests.Application.Executions;

public class SaveUserTaskDraftCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext db, WorkflowExecution execution)> SeedPendingExecutionAsync(string dbName)
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = TestDbContextFactory.Create(currentUser, dbName);

        var workflow = WorkflowDefinition.Create(1, "Approval Workflow", null, 1);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        var version = workflow.CreateNextVersion("""{"nodes":[],"edges":[]}""");
        await db.SaveChangesAsync();

        var execution = WorkflowExecution.CreateQueued(1, version, null);
        execution.MarkPendingApproval("task1", """{"amount":6000}""");
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        return (db, execution);
    }

    [Fact]
    public async Task Handle_PendingExecution_PersistsDraftWithoutResuming()
    {
        var (db, execution) = await SeedPendingExecutionAsync(nameof(Handle_PendingExecution_PersistsDraftWithoutResuming));
        await using var _db = db;

        var handler = new SaveUserTaskDraftCommandHandler(db);
        var result = await handler.Handle(new SaveUserTaskDraftCommand(execution.Id, """{"comment":"in progress"}"""), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var reloaded = await db.WorkflowExecutions.FindAsync(execution.Id);
        Assert.Equal("""{"comment":"in progress"}""", reloaded!.PendingDraftDataJson);
        Assert.Equal(ExecutionStatus.PendingApproval, reloaded.Status);
    }

    [Fact]
    public async Task Handle_ExecutionNotPending_ReturnsConflict()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_ExecutionNotPending_ReturnsConflict));

        var workflow = WorkflowDefinition.Create(1, "WF", null, 1);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();
        var version = workflow.CreateNextVersion("""{"nodes":[],"edges":[]}""");
        await db.SaveChangesAsync();

        var execution = WorkflowExecution.CreateQueued(1, version, null);
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var handler = new SaveUserTaskDraftCommandHandler(db);
        var result = await handler.Handle(new SaveUserTaskDraftCommand(execution.Id, "{}"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }
}
