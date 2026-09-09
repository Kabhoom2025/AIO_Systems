using FlowSphere.Application.Interfaces;

namespace FlowSphere.Tests.TestUtilities;

/// <summary>No-op recorder - tests that care can assert on DispatchCount. Never used outside the
/// test project.</summary>
public class FakeAppNotificationDispatcher : IAppNotificationDispatcher
{
    public int DispatchCount { get; private set; }

    public Task DispatchRecordSubmittedAsync(int organizationId, int appId, string dataJson, CancellationToken cancellationToken)
    {
        DispatchCount++;
        return Task.CompletedTask;
    }
}
