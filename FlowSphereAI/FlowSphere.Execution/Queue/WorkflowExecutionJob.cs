using FlowSphere.Execution.Engine;
using Hangfire;

namespace FlowSphere.Execution.Queue;

/// <summary>Hangfire-invoked wrapper around WorkflowExecutionEngine - registered as a scoped DI
/// service so each job invocation gets its own DbContext/engine instance, same lifetime as a
/// single HTTP request would.</summary>
public class WorkflowExecutionJob
{
    private readonly WorkflowExecutionEngine _engine;

    public WorkflowExecutionJob(WorkflowExecutionEngine engine)
    {
        _engine = engine;
    }

    // The engine already retries individual node failures (see RetryPolicyFactory); retrying the
    // whole execution again at the job level would double-apply that policy and could re-run
    // already-succeeded side effects (e.g. a Slack message already posted), so this is disabled.
    [AutomaticRetry(Attempts = 0)]
    public Task RunAsync(Guid executionId, CancellationToken cancellationToken) =>
        _engine.RunAsync(executionId, cancellationToken);
}
