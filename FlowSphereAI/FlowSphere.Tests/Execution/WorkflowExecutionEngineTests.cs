using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Execution.Engine;
using FlowSphere.Execution.Nodes;
using FlowSphere.Tests.TestUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class WorkflowExecutionEngineTests
{
    private const string LinearGraph = """
        {
            "nodes": [
                { "key": "trigger1", "type": "Trigger", "config": {} },
                { "key": "http1", "type": "HttpRequest", "config": {} }
            ],
            "edges": [
                { "source": "trigger1", "target": "http1" }
            ]
        }
        """;

    [Fact]
    public async Task RunAsync_LinearTwoNodeGraph_AllSucceed_MarksExecutionSucceeded()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(RunAsync_LinearTwoNodeGraph_AllSucceed_MarksExecutionSucceeded));

        var (workflow, version) = await SeedPublishedWorkflowAsync(db, LinearGraph);
        var execution = WorkflowExecution.CreateQueued(1, version, null);
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var registry = new NodeExecutorRegistry(new INodeExecutor[]
        {
            new TriggerWebhookNodeExecutor(),
            new FakeNodeExecutor(WorkflowNodeType.HttpRequest, _ => NodeResult.Ok("{\"ok\":true}"))
        });

        var scopeFactory = new FakeServiceScopeFactory(() => new FakeServiceProvider(new Dictionary<Type, object>
        {
            [typeof(IApplicationDbContext)] = db,
            [typeof(INodeExecutorRegistry)] = registry,
            [typeof(IExecutionNotifier)] = new FakeExecutionNotifier(),
        }));
        var engine = new WorkflowExecutionEngine(db, registry, new FakeExecutionNotifier(), scopeFactory, NullLogger<WorkflowExecutionEngine>.Instance);
        await engine.RunAsync(execution.Id, CancellationToken.None);

        var reloaded = await db.WorkflowExecutions.FindAsync(execution.Id);
        Assert.Equal(ExecutionStatus.Succeeded, reloaded!.Status);

        var steps = db.ExecutionStepLogs.Where(s => s.WorkflowExecutionId == execution.Id).ToList();
        Assert.Equal(2, steps.Count);
        Assert.All(steps, s => Assert.Equal(StepStatus.Succeeded, s.Status));
    }

    [Fact]
    public async Task RunAsync_NodeFails_MarksExecutionFailed_AndStopsWalkingGraph()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(RunAsync_NodeFails_MarksExecutionFailed_AndStopsWalkingGraph));

        var (workflow, version) = await SeedPublishedWorkflowAsync(db, LinearGraph);
        var execution = WorkflowExecution.CreateQueued(1, version, null);
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var registry = new NodeExecutorRegistry(new INodeExecutor[]
        {
            new TriggerWebhookNodeExecutor(),
            new FakeNodeExecutor(WorkflowNodeType.HttpRequest, _ => NodeResult.Fail("simulated downstream failure"))
        });

        var scopeFactory = new FakeServiceScopeFactory(() => new FakeServiceProvider(new Dictionary<Type, object>
        {
            [typeof(IApplicationDbContext)] = db,
            [typeof(INodeExecutorRegistry)] = registry,
            [typeof(IExecutionNotifier)] = new FakeExecutionNotifier(),
        }));
        var engine = new WorkflowExecutionEngine(db, registry, new FakeExecutionNotifier(), scopeFactory, NullLogger<WorkflowExecutionEngine>.Instance);
        await engine.RunAsync(execution.Id, CancellationToken.None);

        var reloaded = await db.WorkflowExecutions.FindAsync(execution.Id);
        Assert.Equal(ExecutionStatus.Failed, reloaded!.Status);
        Assert.Contains("simulated downstream failure", reloaded.ErrorSummary);

        var steps = db.ExecutionStepLogs.Where(s => s.WorkflowExecutionId == execution.Id).ToList();
        Assert.Equal(2, steps.Count);
        Assert.Equal(StepStatus.Succeeded, steps.Single(s => s.NodeKey == "trigger1").Status);
        Assert.Equal(StepStatus.Failed, steps.Single(s => s.NodeKey == "http1").Status);
    }

    private const string TerminateGraph = """
        {
            "nodes": [
                { "key": "trigger1", "type": "Trigger", "config": {} },
                { "key": "end1", "type": "Terminate", "config": {} }
            ],
            "edges": [
                { "source": "trigger1", "target": "end1" }
            ]
        }
        """;

    [Fact]
    public async Task RunAsync_GraphEndingInTerminate_MarksExecutionSucceeded()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(RunAsync_GraphEndingInTerminate_MarksExecutionSucceeded));

        var (workflow, version) = await SeedPublishedWorkflowAsync(db, TerminateGraph);
        var execution = WorkflowExecution.CreateQueued(1, version, null);
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var registry = new NodeExecutorRegistry(new INodeExecutor[] { new TriggerWebhookNodeExecutor() });
        var scopeFactory = new FakeServiceScopeFactory(() => new FakeServiceProvider(new Dictionary<Type, object>
        {
            [typeof(IApplicationDbContext)] = db,
            [typeof(INodeExecutorRegistry)] = registry,
            [typeof(IExecutionNotifier)] = new FakeExecutionNotifier(),
        }));
        var engine = new WorkflowExecutionEngine(db, registry, new FakeExecutionNotifier(), scopeFactory, NullLogger<WorkflowExecutionEngine>.Instance);
        await engine.RunAsync(execution.Id, CancellationToken.None);

        var reloaded = await db.WorkflowExecutions.FindAsync(execution.Id);
        Assert.Equal(ExecutionStatus.Succeeded, reloaded!.Status);

        // Only the Trigger produces a step log - Terminate is intercepted before ExecuteStepAsync,
        // same as UserTask/Loop/Parallel, so it never gets its own ExecutionStepLog row.
        var steps = db.ExecutionStepLogs.Where(s => s.WorkflowExecutionId == execution.Id).ToList();
        Assert.Single(steps);
    }

    private static async Task<(WorkflowDefinition workflow, WorkflowVersion version)> SeedPublishedWorkflowAsync(
        FlowSphere.Infrastructure.Data.FlowSphereDbContext db, string graphJson)
    {
        var workflow = WorkflowDefinition.Create(1, "Test Workflow", null, 1);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        var version = workflow.CreateNextVersion(graphJson);
        await db.SaveChangesAsync();
        workflow.Publish(version.Id);
        await db.SaveChangesAsync();

        return (workflow, version);
    }
}
