using FlowSphere.Application.Common;
using FlowSphere.Application.Executions.Commands.CompleteUserTask;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Tests.TestUtilities;

namespace FlowSphere.Tests.Application.Executions;

public class CompleteUserTaskCommandHandlerTests
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

        db.ExecutionStepLogs.Add(new ExecutionStepLog
        {
            WorkflowExecutionId = execution.Id,
            NodeKey = "task1",
            NodeType = FlowSphere.Domain.Enums.WorkflowNodeType.UserTask,
            Status = StepStatus.WaitingApproval,
            AttemptNumber = 1,
            StartedAt = DateTime.UtcNow,
            InputJson = """{"amount":6000}""",
        });
        await db.SaveChangesAsync();

        return (db, execution);
    }

    [Fact]
    public async Task Handle_Approved_UpdatesStepLogAndEnqueuesResume()
    {
        var (db, execution) = await SeedPendingExecutionAsync(nameof(Handle_Approved_UpdatesStepLogAndEnqueuesResume));
        await using var _ = db;
        var queue = new FakeExecutionQueue();

        var handler = new CompleteUserTaskCommandHandler(db, queue, new FakeCurrentUserContext { OrganizationId = 1 });
        var result = await handler.Handle(new CompleteUserTaskCommand(execution.Id, true, "looks good"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(queue.Enqueued);
        Assert.Equal(execution.Id, queue.Enqueued[0]);

        var reloaded = await db.WorkflowExecutions.FindAsync(execution.Id);
        Assert.Equal("approved", reloaded!.PendingResumeHandle);
        Assert.Equal(ExecutionStatus.Queued, reloaded.Status);

        var stepLog = db.ExecutionStepLogs.Single(s => s.WorkflowExecutionId == execution.Id && s.NodeKey == "task1");
        Assert.Equal(StepStatus.Succeeded, stepLog.Status);
        Assert.Contains("\"approved\":true", stepLog.OutputJson);
        Assert.Contains("looks good", stepLog.OutputJson);
    }

    [Fact]
    public async Task Handle_Rejected_SetsRejectedHandle()
    {
        var (db, execution) = await SeedPendingExecutionAsync(nameof(Handle_Rejected_SetsRejectedHandle));
        await using var _ = db;
        var queue = new FakeExecutionQueue();

        var handler = new CompleteUserTaskCommandHandler(db, queue, new FakeCurrentUserContext { OrganizationId = 1 });
        var result = await handler.Handle(new CompleteUserTaskCommand(execution.Id, false, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var reloaded = await db.WorkflowExecutions.FindAsync(execution.Id);
        Assert.Equal("rejected", reloaded!.PendingResumeHandle);
    }

    [Fact]
    public async Task Handle_ExecutionNotPendingApproval_ReturnsConflict()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_ExecutionNotPendingApproval_ReturnsConflict));

        var workflow = WorkflowDefinition.Create(1, "WF", null, 1);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();
        var version = workflow.CreateNextVersion("""{"nodes":[],"edges":[]}""");
        await db.SaveChangesAsync();

        var execution = WorkflowExecution.CreateQueued(1, version, null);
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var handler = new CompleteUserTaskCommandHandler(db, new FakeExecutionQueue(), new FakeCurrentUserContext { OrganizationId = 1 });
        var result = await handler.Handle(new CompleteUserTaskCommand(execution.Id, true, null), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext db, WorkflowExecution execution, int hrUserId, int employeeUserId)> SeedRoleRestrictedExecutionAsync(string dbName)
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = TestDbContextFactory.Create(currentUser, dbName);

        var hrRole = new Role { OrganizationId = 1, Name = "HR", Permissions = "workflows.approve" };
        var employeeRole = new Role { OrganizationId = 1, Name = "Employee", Permissions = "workflows.approve" };
        db.Roles.AddRange(hrRole, employeeRole);
        await db.SaveChangesAsync();

        var hrUser = new User { OrganizationId = 1, Name = "Harper", Email = $"harper-{dbName}@acme.test", PasswordHash = "x", RoleId = hrRole.Id, IsActive = true, IsEmailVerified = true };
        var employeeUser = new User { OrganizationId = 1, Name = "Jordan", Email = $"jordan-{dbName}@acme.test", PasswordHash = "x", RoleId = employeeRole.Id, IsActive = true, IsEmailVerified = true };
        db.Users.AddRange(hrUser, employeeUser);
        await db.SaveChangesAsync();

        var workflow = WorkflowDefinition.Create(1, "Approval Workflow", null, 1);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        var graphJson = """{"nodes":[{"key":"task1","type":"UserTask","config":{"roles":["HR"]}}],"edges":[]}""";
        var version = workflow.CreateNextVersion(graphJson);
        await db.SaveChangesAsync();

        var execution = WorkflowExecution.CreateQueued(1, version, null);
        execution.MarkPendingApproval("task1", """{"amount":6000}""");
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        db.ExecutionStepLogs.Add(new ExecutionStepLog
        {
            WorkflowExecutionId = execution.Id,
            NodeKey = "task1",
            NodeType = WorkflowNodeType.UserTask,
            Status = StepStatus.WaitingApproval,
            AttemptNumber = 1,
            StartedAt = DateTime.UtcNow,
            InputJson = """{"amount":6000}""",
        });
        await db.SaveChangesAsync();

        return (db, execution, hrUser.Id, employeeUser.Id);
    }

    [Fact]
    public async Task Handle_UserWithAllowedRole_Succeeds()
    {
        var (db, execution, hrUserId, _) = await SeedRoleRestrictedExecutionAsync(nameof(Handle_UserWithAllowedRole_Succeeds));
        await using var _db = db;

        var handler = new CompleteUserTaskCommandHandler(db, new FakeExecutionQueue(), new FakeCurrentUserContext { OrganizationId = 1, UserId = hrUserId });
        var result = await handler.Handle(new CompleteUserTaskCommand(execution.Id, true, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_UserWithoutAllowedRole_ReturnsUnauthorized()
    {
        var (db, execution, _, employeeUserId) = await SeedRoleRestrictedExecutionAsync(nameof(Handle_UserWithoutAllowedRole_ReturnsUnauthorized));
        await using var _db = db;

        var handler = new CompleteUserTaskCommandHandler(db, new FakeExecutionQueue(), new FakeCurrentUserContext { OrganizationId = 1, UserId = employeeUserId });
        var result = await handler.Handle(new CompleteUserTaskCommand(execution.Id, true, null), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_NoRolesConfigured_AnyoneCanResolve()
    {
        // The pre-existing SeedPendingExecutionAsync graph has no "roles" config at all - proves
        // every workflow authored before this feature existed keeps working unrestricted.
        var (db, execution) = await SeedPendingExecutionAsync(nameof(Handle_NoRolesConfigured_AnyoneCanResolve));
        await using var _db = db;

        var handler = new CompleteUserTaskCommandHandler(db, new FakeExecutionQueue(), new FakeCurrentUserContext { OrganizationId = 1, UserId = 999 });
        var result = await handler.Handle(new CompleteUserTaskCommand(execution.Id, true, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
