using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Tests.TestUtilities;

/// <summary>No-op recorder - tests that care can assert on DispatchCount. Never used outside the
/// test project.</summary>
public class FakeAppTriggerDispatcher : IAppTriggerDispatcher
{
    public int DispatchCount { get; private set; }

    public Task DispatchRecordSubmittedAsync(int organizationId, int sourceAppId, EnvironmentStage stage, string dataJson, CancellationToken cancellationToken)
    {
        DispatchCount++;
        return Task.CompletedTask;
    }
}
