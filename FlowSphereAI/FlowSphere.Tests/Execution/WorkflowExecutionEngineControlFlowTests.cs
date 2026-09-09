using System.Diagnostics;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Execution.Engine;
using FlowSphere.Execution.Nodes;
using FlowSphere.Tests.TestUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class WorkflowExecutionEngineControlFlowTests
{
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

    private static WorkflowExecutionEngine CreateEngine(
        FlowSphere.Infrastructure.Data.FlowSphereDbContext db, NodeExecutorRegistry registry, IExecutionNotifier? notifier = null)
    {
        var scopeFactory = new FakeServiceScopeFactory(() => new FakeServiceProvider(new Dictionary<Type, object>
        {
            [typeof(IApplicationDbContext)] = db,
            [typeof(INodeExecutorRegistry)] = registry,
            [typeof(IExecutionNotifier)] = notifier ?? new FakeExecutionNotifier(),
        }));

        return new WorkflowExecutionEngine(db, registry, notifier ?? new FakeExecutionNotifier(), scopeFactory, NullLogger<WorkflowExecutionEngine>.Instance);
    }

    [Fact]
    public async Task Decision_RoutesToMatchingCaseHandle()
    {
        const string graph = """
            {
                "nodes": [
                    { "key": "trigger1", "type": "Trigger", "config": {} },
                    { "key": "decision1", "type": "Decision", "config": {
                        "field": "type", "operator": "equals",
                        "cases": [ { "value": "a", "handle": "caseA" }, { "value": "b", "handle": "caseB" } ],
                        "defaultHandle": "caseDefault"
                    } },
                    { "key": "httpA", "type": "HttpRequest", "config": {} },
                    { "key": "httpB", "type": "HttpRequest", "config": {} },
                    { "key": "httpDefault", "type": "HttpRequest", "config": {} }
                ],
                "edges": [
                    { "source": "trigger1", "target": "decision1" },
                    { "source": "decision1", "target": "httpA", "sourceHandle": "caseA" },
                    { "source": "decision1", "target": "httpB", "sourceHandle": "caseB" },
                    { "source": "decision1", "target": "httpDefault", "sourceHandle": "caseDefault" }
                ]
            }
            """;

        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Decision_RoutesToMatchingCaseHandle));
        var (_, version) = await SeedPublishedWorkflowAsync(db, graph);
        var execution = WorkflowExecution.CreateQueued(1, version, """{"type":"b"}""");
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var registry = new NodeExecutorRegistry(new INodeExecutor[]
        {
            new TriggerWebhookNodeExecutor(),
            new DecisionNodeExecutor(),
            new FakeNodeExecutor(WorkflowNodeType.HttpRequest, _ => NodeResult.Ok("{}")),
        });

        await CreateEngine(db, registry).RunAsync(execution.Id, CancellationToken.None);

        var reloaded = await db.WorkflowExecutions.FindAsync(execution.Id);
        Assert.Equal(ExecutionStatus.Succeeded, reloaded!.Status);

        var steps = db.ExecutionStepLogs.Where(s => s.WorkflowExecutionId == execution.Id).Select(s => s.NodeKey).ToList();
        Assert.Contains("httpB", steps);
        Assert.DoesNotContain("httpA", steps);
        Assert.DoesNotContain("httpDefault", steps);
    }

    [Fact]
    public async Task Decision_RoutesToDefaultHandle_WhenNoCaseMatches()
    {
        const string graph = """
            {
                "nodes": [
                    { "key": "trigger1", "type": "Trigger", "config": {} },
                    { "key": "decision1", "type": "Decision", "config": {
                        "field": "type", "operator": "equals",
                        "cases": [ { "value": "a", "handle": "caseA" } ],
                        "defaultHandle": "caseDefault"
                    } },
                    { "key": "httpA", "type": "HttpRequest", "config": {} },
                    { "key": "httpDefault", "type": "HttpRequest", "config": {} }
                ],
                "edges": [
                    { "source": "trigger1", "target": "decision1" },
                    { "source": "decision1", "target": "httpA", "sourceHandle": "caseA" },
                    { "source": "decision1", "target": "httpDefault", "sourceHandle": "caseDefault" }
                ]
            }
            """;

        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Decision_RoutesToDefaultHandle_WhenNoCaseMatches));
        var (_, version) = await SeedPublishedWorkflowAsync(db, graph);
        var execution = WorkflowExecution.CreateQueued(1, version, """{"type":"z"}""");
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var registry = new NodeExecutorRegistry(new INodeExecutor[]
        {
            new TriggerWebhookNodeExecutor(),
            new DecisionNodeExecutor(),
            new FakeNodeExecutor(WorkflowNodeType.HttpRequest, _ => NodeResult.Ok("{}")),
        });

        await CreateEngine(db, registry).RunAsync(execution.Id, CancellationToken.None);

        var steps = db.ExecutionStepLogs.Where(s => s.WorkflowExecutionId == execution.Id).Select(s => s.NodeKey).ToList();
        Assert.Contains("httpDefault", steps);
        Assert.DoesNotContain("httpA", steps);
    }

    [Fact]
    public async Task Exception_RoutesToErrorHandler_InsteadOfFailingExecution()
    {
        const string graph = """
            {
                "nodes": [
                    { "key": "trigger1", "type": "Trigger", "config": {} },
                    { "key": "http1", "type": "HttpRequest", "config": {} },
                    { "key": "exception1", "type": "Exception", "config": {} },
                    { "key": "notify1", "type": "HttpRequest", "config": {} }
                ],
                "edges": [
                    { "source": "trigger1", "target": "http1" },
                    { "source": "http1", "target": "exception1", "sourceHandle": "error" },
                    { "source": "exception1", "target": "notify1" }
                ]
            }
            """;

        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Exception_RoutesToErrorHandler_InsteadOfFailingExecution));
        var (_, version) = await SeedPublishedWorkflowAsync(db, graph);
        var execution = WorkflowExecution.CreateQueued(1, version, null);
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var registry = new NodeExecutorRegistry(new INodeExecutor[]
        {
            new TriggerWebhookNodeExecutor(),
            new FakeNodeExecutor(WorkflowNodeType.HttpRequest, ctx => ctx.NodeKey == "http1"
                ? NodeResult.Fail("simulated downstream failure")
                : NodeResult.Ok("{}")),
            new ExceptionNodeExecutor(),
        });

        await CreateEngine(db, registry).RunAsync(execution.Id, CancellationToken.None);

        var reloaded = await db.WorkflowExecutions.FindAsync(execution.Id);
        Assert.Equal(ExecutionStatus.Succeeded, reloaded!.Status);

        var steps = db.ExecutionStepLogs.Where(s => s.WorkflowExecutionId == execution.Id).ToList();
        Assert.Equal(StepStatus.Failed, steps.Single(s => s.NodeKey == "http1").Status);
        Assert.Equal(StepStatus.Succeeded, steps.Single(s => s.NodeKey == "exception1").Status);
        Assert.Equal(StepStatus.Succeeded, steps.Single(s => s.NodeKey == "notify1").Status);
    }

    [Fact]
    public async Task Loop_IteratesArrayOverBodyNode_AndAggregatesResults()
    {
        const string graph = """
            {
                "nodes": [
                    { "key": "trigger1", "type": "Trigger", "config": {} },
                    { "key": "loop1", "type": "Loop", "config": { "arrayPath": "items" } },
                    { "key": "body1", "type": "HttpRequest", "config": {} },
                    { "key": "after1", "type": "HttpRequest", "config": {} }
                ],
                "edges": [
                    { "source": "trigger1", "target": "loop1" },
                    { "source": "loop1", "target": "body1" },
                    { "source": "body1", "target": "after1" }
                ]
            }
            """;

        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Loop_IteratesArrayOverBodyNode_AndAggregatesResults));
        var (_, version) = await SeedPublishedWorkflowAsync(db, graph);
        var execution = WorkflowExecution.CreateQueued(1, version, """{"items":["a","b","c"]}""");
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var registry = new NodeExecutorRegistry(new INodeExecutor[]
        {
            new TriggerWebhookNodeExecutor(),
            new FakeNodeExecutor(WorkflowNodeType.HttpRequest, ctx => NodeResult.Ok($$"""{"echo":{{ctx.InputJson}}}""")),
        });

        await CreateEngine(db, registry).RunAsync(execution.Id, CancellationToken.None);

        var reloaded = await db.WorkflowExecutions.FindAsync(execution.Id);
        Assert.Equal(ExecutionStatus.Succeeded, reloaded!.Status);

        var bodySteps = db.ExecutionStepLogs.Where(s => s.WorkflowExecutionId == execution.Id && s.NodeKey == "body1").ToList();
        Assert.Equal(3, bodySteps.Count); // body executed once per array item

        var afterStep = db.ExecutionStepLogs.Single(s => s.WorkflowExecutionId == execution.Id && s.NodeKey == "after1");
        Assert.Equal(StepStatus.Succeeded, afterStep.Status);
        // "after" receives the loop's aggregated array as input, one element per iteration
        Assert.Contains("\"echo\":\"a\"", afterStep.InputJson);
        Assert.Contains("\"echo\":\"c\"", afterStep.InputJson);
    }

    [Fact]
    public async Task Parallel_RunsBranchesConcurrently_AndMergesResults()
    {
        const string graph = """
            {
                "nodes": [
                    { "key": "trigger1", "type": "Trigger", "config": {} },
                    { "key": "parallel1", "type": "Parallel", "config": { "mergeNodeKey": "merge1" } },
                    { "key": "branchA", "type": "HttpRequest", "config": {} },
                    { "key": "branchB", "type": "HttpRequest", "config": {} },
                    { "key": "merge1", "type": "Merge", "config": {} },
                    { "key": "after1", "type": "HttpRequest", "config": {} }
                ],
                "edges": [
                    { "source": "trigger1", "target": "parallel1" },
                    { "source": "parallel1", "target": "branchA" },
                    { "source": "parallel1", "target": "branchB" },
                    { "source": "branchA", "target": "merge1" },
                    { "source": "branchB", "target": "merge1" },
                    { "source": "merge1", "target": "after1" }
                ]
            }
            """;

        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        const string dbName = nameof(Parallel_RunsBranchesConcurrently_AndMergesResults);
        await using var db = TestDbContextFactory.Create(currentUser, dbName);
        var (_, version) = await SeedPublishedWorkflowAsync(db, graph);
        var execution = WorkflowExecution.CreateQueued(1, version, null);
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var branchDelay = TimeSpan.FromMilliseconds(150);
        var registry = new NodeExecutorRegistry(new INodeExecutor[]
        {
            new TriggerWebhookNodeExecutor(),
            new FakeNodeExecutor(WorkflowNodeType.HttpRequest, async (ctx, ct) =>
            {
                if (ctx.NodeKey is "branchA" or "branchB")
                {
                    await Task.Delay(branchDelay, ct);
                }
                return NodeResult.Ok($"\"{ctx.NodeKey}-done\"");
            }),
            new MergeNodeExecutor(),
        });

        // Each Parallel branch gets its own scoped DbContext (see WorkflowExecutionEngine.RunBranchAsync) -
        // point every scope at the SAME InMemory database name so branches and the main run see
        // consistent data, exactly like separate real DbContext instances against one Postgres db.
        var scopeFactory = new FakeServiceScopeFactory(() => new FakeServiceProvider(new Dictionary<Type, object>
        {
            [typeof(IApplicationDbContext)] = TestDbContextFactory.Create(currentUser, dbName),
            [typeof(INodeExecutorRegistry)] = registry,
            [typeof(IExecutionNotifier)] = new FakeExecutionNotifier(),
        }));
        var engine = new WorkflowExecutionEngine(db, registry, new FakeExecutionNotifier(), scopeFactory, NullLogger<WorkflowExecutionEngine>.Instance);

        var stopwatch = Stopwatch.StartNew();
        await engine.RunAsync(execution.Id, CancellationToken.None);
        stopwatch.Stop();

        var reloaded = await db.WorkflowExecutions.FindAsync(execution.Id);
        Assert.Equal(ExecutionStatus.Succeeded, reloaded!.Status);

        // If the branches ran sequentially this would take >= 2x branchDelay; concurrently it
        // should take roughly 1x branchDelay plus overhead - assert well under the sequential sum
        // to prove real concurrency, not a fake/sequential "parallel" node.
        Assert.True(stopwatch.Elapsed < branchDelay * 2, $"Expected concurrent execution, took {stopwatch.Elapsed} (2x branch delay = {branchDelay * 2})");

        var mergeStep = db.ExecutionStepLogs.Single(s => s.WorkflowExecutionId == execution.Id && s.NodeKey == "merge1");
        Assert.Equal(StepStatus.Succeeded, mergeStep.Status);
        Assert.Contains("branchA-done", mergeStep.InputJson);
        Assert.Contains("branchB-done", mergeStep.InputJson);

        var afterStep = db.ExecutionStepLogs.Single(s => s.WorkflowExecutionId == execution.Id && s.NodeKey == "after1");
        Assert.Equal(StepStatus.Succeeded, afterStep.Status);
    }

    [Fact]
    public async Task UserTask_SuspendsExecution_WithPendingApprovalStatus()
    {
        const string graph = """
            {
                "nodes": [
                    { "key": "trigger1", "type": "Trigger", "config": {} },
                    { "key": "task1", "type": "UserTask", "config": {} },
                    { "key": "approved1", "type": "HttpRequest", "config": {} },
                    { "key": "rejected1", "type": "HttpRequest", "config": {} }
                ],
                "edges": [
                    { "source": "trigger1", "target": "task1" },
                    { "source": "task1", "target": "approved1", "sourceHandle": "approved" },
                    { "source": "task1", "target": "rejected1", "sourceHandle": "rejected" }
                ]
            }
            """;

        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(UserTask_SuspendsExecution_WithPendingApprovalStatus));
        var (_, version) = await SeedPublishedWorkflowAsync(db, graph);
        var execution = WorkflowExecution.CreateQueued(1, version, """{"amount":6000}""");
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var registry = new NodeExecutorRegistry(new INodeExecutor[]
        {
            new TriggerWebhookNodeExecutor(),
            new FakeNodeExecutor(WorkflowNodeType.HttpRequest, _ => NodeResult.Ok("{}")),
        });

        await CreateEngine(db, registry).RunAsync(execution.Id, CancellationToken.None);

        var reloaded = await db.WorkflowExecutions.FindAsync(execution.Id);
        Assert.Equal(ExecutionStatus.PendingApproval, reloaded!.Status);
        Assert.Equal("task1", reloaded.PendingNodeKey);

        var taskStep = db.ExecutionStepLogs.Single(s => s.WorkflowExecutionId == execution.Id && s.NodeKey == "task1");
        Assert.Equal(StepStatus.WaitingApproval, taskStep.Status);

        var downstreamSteps = db.ExecutionStepLogs.Where(s => s.WorkflowExecutionId == execution.Id && s.NodeKey != "trigger1" && s.NodeKey != "task1").ToList();
        Assert.Empty(downstreamSteps); // nothing past the User Task should have run yet
    }

    [Fact]
    public async Task UserTask_ResumesToApprovedHandle_AfterDecisionIsRecorded()
    {
        const string graph = """
            {
                "nodes": [
                    { "key": "trigger1", "type": "Trigger", "config": {} },
                    { "key": "task1", "type": "UserTask", "config": {} },
                    { "key": "approved1", "type": "HttpRequest", "config": {} },
                    { "key": "rejected1", "type": "HttpRequest", "config": {} }
                ],
                "edges": [
                    { "source": "trigger1", "target": "task1" },
                    { "source": "task1", "target": "approved1", "sourceHandle": "approved" },
                    { "source": "task1", "target": "rejected1", "sourceHandle": "rejected" }
                ]
            }
            """;

        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(UserTask_ResumesToApprovedHandle_AfterDecisionIsRecorded));
        var (_, version) = await SeedPublishedWorkflowAsync(db, graph);
        var execution = WorkflowExecution.CreateQueued(1, version, null);
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync();

        var registry = new NodeExecutorRegistry(new INodeExecutor[]
        {
            new TriggerWebhookNodeExecutor(),
            new FakeNodeExecutor(WorkflowNodeType.HttpRequest, _ => NodeResult.Ok("{}")),
        });
        var engine = CreateEngine(db, registry);

        await engine.RunAsync(execution.Id, CancellationToken.None);

        // Simulate exactly what CompleteUserTaskCommandHandler does for an "approved" decision.
        var taskStep = db.ExecutionStepLogs.Single(s => s.WorkflowExecutionId == execution.Id && s.NodeKey == "task1");
        taskStep.Status = StepStatus.Succeeded;
        taskStep.OutputJson = """{"approved":true,"comment":null}""";
        taskStep.CompletedAt = DateTime.UtcNow;

        var pending = await db.WorkflowExecutions.FindAsync(execution.Id);
        pending!.PendingResumeHandle = "approved";
        await db.SaveChangesAsync();

        await engine.RunAsync(execution.Id, CancellationToken.None);

        var reloaded = await db.WorkflowExecutions.FindAsync(execution.Id);
        Assert.Equal(ExecutionStatus.Succeeded, reloaded!.Status);
        Assert.Null(reloaded.PendingNodeKey);
        Assert.Null(reloaded.PendingResumeHandle);

        var steps = db.ExecutionStepLogs.Where(s => s.WorkflowExecutionId == execution.Id).Select(s => s.NodeKey).ToList();
        Assert.Contains("approved1", steps);
        Assert.DoesNotContain("rejected1", steps);
    }
}
