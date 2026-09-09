namespace FlowSphere.Application.Interfaces;

/// <summary>Pushes live execution state to subscribers (SignalR in M5; a no-op in M3/M4 so the
/// engine has a real implementation to call against from day one).</summary>
public interface IExecutionNotifier
{
    Task NotifyExecutionStartedAsync(int organizationId, Guid executionId, CancellationToken cancellationToken);
    Task NotifyStepUpdatedAsync(int organizationId, Guid executionId, int stepLogId, string nodeKey, string status, CancellationToken cancellationToken);
    Task NotifyExecutionCompletedAsync(int organizationId, Guid executionId, string status, CancellationToken cancellationToken);
}
