using FlowSphere.Application.Interfaces;

namespace FlowSphere.Tests.TestUtilities;

public class FakeExecutionNotifier : IExecutionNotifier
{
    public Task NotifyExecutionStartedAsync(int organizationId, Guid executionId, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task NotifyStepUpdatedAsync(int organizationId, Guid executionId, int stepLogId, string nodeKey, string status, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task NotifyExecutionCompletedAsync(int organizationId, Guid executionId, string status, CancellationToken cancellationToken) => Task.CompletedTask;
}
