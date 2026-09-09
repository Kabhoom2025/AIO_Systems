using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProjectFlowAI.Application.Features.Automation;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Unit;

/// <summary>Exercises the RequireApproval pause/resume semantics end-to-end against a real
/// WorkflowEngine + InMemory db: a run with a RequireApproval action stops at AwaitingApproval and
/// does NOT execute subsequent actions until approved; approving resumes and executes the rest;
/// rejecting marks the run Failed and does not execute the rest.</summary>
public class WorkflowApprovalFlowTests
{
    private static WorkflowEngine CreateEngine(Infrastructure.Data.ProjectFlowDbContext db, FakeDateTimeProvider? clock = null)
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
        return new WorkflowEngine(db, new NoOpNotificationDispatcher(), httpClientFactory.Object,
            clock ?? new FakeDateTimeProvider(), NullLogger<WorkflowEngine>.Instance);
    }

    private static async Task<(Infrastructure.Data.ProjectFlowDbContext Db, Project Project, WorkItem Item, Guid ApproverId)> SeedAsync()
    {
        var db = InMemoryDbContextFactory.Create();
        var org = new Organization { Name = "Org", Slug = "org" };
        var approver = new User { Email = "approver@example.com", PasswordHash = "x", FirstName = "A", LastName = "P" };
        db.Organizations.Add(org);
        db.Users.Add(approver);
        await db.SaveChangesAsync();

        var project = new Project { OrganizationId = org.Id, Key = "PFA", Name = "Test Project", OwnerUserId = approver.Id };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var item = new WorkItem
        {
            ProjectId = project.Id, Title = "Ship it", Status = WorkItemStatus.InProgress,
            Priority = WorkItemPriority.High, ReporterUserId = approver.Id
        };
        db.WorkItems.Add(item);
        await db.SaveChangesAsync();

        return (db, project, item, approver.Id);
    }

    private static WorkflowDefinition BuildWorkflowWithApprovalThenChangeStatus(Guid organizationId, Guid projectId, Guid approverId)
    {
        var workflow = new WorkflowDefinition
        {
            OrganizationId = organizationId,
            ProjectId = projectId,
            Name = "Approve then close",
            TriggerType = WorkflowTriggerType.WorkItemStatusChanged,
            IsEnabled = true,
            CreatedByUserId = approverId
        };
        workflow.Actions.Add(new WorkflowAction
        {
            ActionType = WorkflowActionType.RequireApproval,
            ActionConfigJson = JsonSerializer.Serialize(new { approverUserId = approverId.ToString() }),
            Order = 0
        });
        workflow.Actions.Add(new WorkflowAction
        {
            ActionType = WorkflowActionType.ChangeStatus,
            ActionConfigJson = JsonSerializer.Serialize(new { status = "Done" }),
            Order = 1
        });
        return workflow;
    }

    [Fact]
    public async Task Run_With_RequireApproval_Action_Stops_At_AwaitingApproval_And_Does_Not_Run_Subsequent_Actions()
    {
        var (db, project, item, approverId) = await SeedAsync();
        var workflow = BuildWorkflowWithApprovalThenChangeStatus(project.OrganizationId, project.Id, approverId);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        var engine = CreateEngine(db);
        await engine.EvaluateTriggerAsync(WorkflowTriggerType.WorkItemStatusChanged, project.Id, item);

        var run = await db.WorkflowRuns.SingleAsync(r => r.WorkflowDefinitionId == workflow.Id);
        Assert.Equal(WorkflowRunStatus.AwaitingApproval, run.Status);

        var approval = await db.WorkflowApprovalRequests.SingleAsync(a => a.WorkflowRunId == run.Id);
        Assert.Equal(WorkflowApprovalStatus.Pending, approval.Status);
        Assert.Equal(approverId, approval.RequestedApproverUserId);

        // The subsequent ChangeStatus action must NOT have executed yet.
        var reloadedItem = await db.WorkItems.SingleAsync(w => w.Id == item.Id);
        Assert.Equal(WorkItemStatus.InProgress, reloadedItem.Status);
    }

    [Fact]
    public async Task Approving_Resumes_The_Run_And_Executes_Remaining_Actions()
    {
        var (db, project, item, approverId) = await SeedAsync();
        var workflow = BuildWorkflowWithApprovalThenChangeStatus(project.OrganizationId, project.Id, approverId);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        var engine = CreateEngine(db);
        await engine.EvaluateTriggerAsync(WorkflowTriggerType.WorkItemStatusChanged, project.Id, item);
        var run = await db.WorkflowRuns.SingleAsync(r => r.WorkflowDefinitionId == workflow.Id);

        await engine.ResumeRunAsync(run.Id, approved: true);

        var reloadedRun = await db.WorkflowRuns.SingleAsync(r => r.Id == run.Id);
        Assert.Equal(WorkflowRunStatus.Succeeded, reloadedRun.Status);
        Assert.NotNull(reloadedRun.CompletedAt);

        var reloadedItem = await db.WorkItems.SingleAsync(w => w.Id == item.Id);
        Assert.Equal(WorkItemStatus.Done, reloadedItem.Status);
    }

    [Fact]
    public async Task Rejecting_Marks_The_Run_Failed_And_Does_Not_Execute_Remaining_Actions()
    {
        var (db, project, item, approverId) = await SeedAsync();
        var workflow = BuildWorkflowWithApprovalThenChangeStatus(project.OrganizationId, project.Id, approverId);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        var engine = CreateEngine(db);
        await engine.EvaluateTriggerAsync(WorkflowTriggerType.WorkItemStatusChanged, project.Id, item);
        var run = await db.WorkflowRuns.SingleAsync(r => r.WorkflowDefinitionId == workflow.Id);

        await engine.ResumeRunAsync(run.Id, approved: false);

        var reloadedRun = await db.WorkflowRuns.SingleAsync(r => r.Id == run.Id);
        Assert.Equal(WorkflowRunStatus.Failed, reloadedRun.Status);
        Assert.NotNull(reloadedRun.CompletedAt);

        // ChangeStatus must never have executed.
        var reloadedItem = await db.WorkItems.SingleAsync(w => w.Id == item.Id);
        Assert.Equal(WorkItemStatus.InProgress, reloadedItem.Status);
    }

    [Fact]
    public async Task Workflow_With_No_Matching_Condition_Never_Creates_A_Run()
    {
        var (db, project, item, approverId) = await SeedAsync();
        var workflow = BuildWorkflowWithApprovalThenChangeStatus(project.OrganizationId, project.Id, approverId);
        workflow.Conditions.Add(new WorkflowCondition { FieldPath = "Priority", Operator = WorkflowConditionOperator.Equals, Value = "Lowest" });
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        var engine = CreateEngine(db);
        await engine.EvaluateTriggerAsync(WorkflowTriggerType.WorkItemStatusChanged, project.Id, item);

        Assert.False(await db.WorkflowRuns.AnyAsync(r => r.WorkflowDefinitionId == workflow.Id));
    }
}
