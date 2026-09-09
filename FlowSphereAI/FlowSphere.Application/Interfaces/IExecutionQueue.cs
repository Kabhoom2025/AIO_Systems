namespace FlowSphere.Application.Interfaces;

/// <summary>Durable background dispatch for a queued workflow execution - backed by Hangfire
/// (Redis storage), so a queued execution survives an API process restart instead of being lost
/// with an in-memory channel. ExecuteWorkflowCommandHandler only depends on this interface.</summary>
public interface IExecutionQueue
{
    Task EnqueueAsync(Guid executionId, CancellationToken cancellationToken);
}
