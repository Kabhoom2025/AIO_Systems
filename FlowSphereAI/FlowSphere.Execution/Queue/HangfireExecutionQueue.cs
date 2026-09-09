using FlowSphere.Application.Interfaces;
using Hangfire;

namespace FlowSphere.Execution.Queue;

/// <summary>Enqueues a durable Hangfire job (Redis-backed storage) instead of writing to an
/// in-process channel - the job survives an API restart and is picked up by any Hangfire server
/// instance, not just the one that enqueued it (true horizontal scale-out).</summary>
public class HangfireExecutionQueue : IExecutionQueue
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireExecutionQueue(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public Task EnqueueAsync(Guid executionId, CancellationToken cancellationToken)
    {
        // Hangfire serializes the expression tree, not a live closure - CancellationToken.None
        // is a placeholder here (the token that eventually runs the job is Hangfire's own
        // shutdown token, not this one); this call only needs to survive serialization.
        _backgroundJobClient.Enqueue<WorkflowExecutionJob>(job => job.RunAsync(executionId, CancellationToken.None));
        return Task.CompletedTask;
    }
}
