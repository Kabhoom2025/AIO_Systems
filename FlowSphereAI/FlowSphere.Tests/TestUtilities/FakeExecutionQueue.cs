using FlowSphere.Application.Interfaces;

namespace FlowSphere.Tests.TestUtilities;

public class FakeExecutionQueue : IExecutionQueue
{
    public List<Guid> Enqueued { get; } = new();

    public Task EnqueueAsync(Guid executionId, CancellationToken cancellationToken)
    {
        Enqueued.Add(executionId);
        return Task.CompletedTask;
    }
}
