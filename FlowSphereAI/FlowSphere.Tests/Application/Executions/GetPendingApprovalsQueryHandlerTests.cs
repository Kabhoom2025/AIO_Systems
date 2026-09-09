using FlowSphere.Application.Executions.Queries.GetPendingApprovals;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Executions;

public class GetPendingApprovalsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ExecutionPendingApproval_ReturnsIt()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_ExecutionPendingApproval_ReturnsIt));

        var workflow = WorkflowDefinition.Create(1, "Leave Request Approval", null, 1);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        var version = workflow.CreateNextVersion(
            """{"nodes":[{"key":"review","type":"UserTask","config":{"assigneeLabel":"HR"}}],"edges":[]}""");
        await db.SaveChangesAsync();

        var execution = WorkflowExecution.CreateQueued(1, version, null, "AppLaunch");
        execution.MarkPendingApproval("review", null);
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var handler = new GetPendingApprovalsQueryHandler(db);
        var result = await handler.Handle(new GetPendingApprovalsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(execution.Id, item.Id);
        Assert.Equal(workflow.Id, item.WorkflowDefinitionId);
        Assert.Equal("Leave Request Approval", item.WorkflowName);
        Assert.Equal("AppLaunch", item.TriggerSource);
        Assert.Equal("HR", item.AssigneeLabel);
    }

    [Fact]
    public async Task Handle_WorkflowDefinitionIdFilterProvided_ReturnsOnlyMatchingWorkflow()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_WorkflowDefinitionIdFilterProvided_ReturnsOnlyMatchingWorkflow));

        var leaveWorkflow = WorkflowDefinition.Create(1, "Leave Request Approval", null, 1);
        var expenseWorkflow = WorkflowDefinition.Create(1, "Expense Approval", null, 1);
        db.WorkflowDefinitions.AddRange(leaveWorkflow, expenseWorkflow);
        await db.SaveChangesAsync();

        var leaveVersion = leaveWorkflow.CreateNextVersion("""{"nodes":[],"edges":[]}""");
        var expenseVersion = expenseWorkflow.CreateNextVersion("""{"nodes":[],"edges":[]}""");
        await db.SaveChangesAsync();

        var leaveExecution = WorkflowExecution.CreateQueued(1, leaveVersion, null, "AppLaunch");
        leaveExecution.MarkPendingApproval("review", null);
        var expenseExecution = WorkflowExecution.CreateQueued(1, expenseVersion, null, "AppLaunch");
        expenseExecution.MarkPendingApproval("review", null);
        db.WorkflowExecutions.AddRange(leaveExecution, expenseExecution);
        await db.SaveChangesAsync();

        var handler = new GetPendingApprovalsQueryHandler(db);
        var result = await handler.Handle(new GetPendingApprovalsQuery(WorkflowDefinitionId: leaveWorkflow.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(leaveExecution.Id, item.Id);
        Assert.Equal(leaveWorkflow.Id, item.WorkflowDefinitionId);
    }

    [Fact]
    public async Task Handle_WorkflowDefinitionIdFilterMatchesNothing_ReturnsEmpty()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_WorkflowDefinitionIdFilterMatchesNothing_ReturnsEmpty));

        var workflow = WorkflowDefinition.Create(1, "Leave Request Approval", null, 1);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        var version = workflow.CreateNextVersion("""{"nodes":[],"edges":[]}""");
        await db.SaveChangesAsync();

        var execution = WorkflowExecution.CreateQueued(1, version, null, "AppLaunch");
        execution.MarkPendingApproval("review", null);
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var handler = new GetPendingApprovalsQueryHandler(db);
        var result = await handler.Handle(new GetPendingApprovalsQuery(WorkflowDefinitionId: workflow.Id + 999), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }

    [Fact]
    public async Task Handle_NoExecutionsPendingApproval_ReturnsEmpty()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_NoExecutionsPendingApproval_ReturnsEmpty));

        var workflow = WorkflowDefinition.Create(1, "Leave Request Approval", null, 1);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        var version = workflow.CreateNextVersion("""{"nodes":[],"edges":[]}""");
        await db.SaveChangesAsync();

        var execution = WorkflowExecution.CreateQueued(1, version, null);
        execution.MarkRunning();
        execution.MarkSucceeded();
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var handler = new GetPendingApprovalsQueryHandler(db);
        var result = await handler.Handle(new GetPendingApprovalsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }
}
