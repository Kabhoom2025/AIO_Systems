using FlowSphere.Domain.Enums;

namespace FlowSphere.Application.Interfaces;

/// <summary>Evaluates and executes an app's configured Triggers (cross-app automation) when a
/// record is submitted - the only firing point wired up today, see AppTrigger's doc comment.</summary>
public interface IAppTriggerDispatcher
{
    Task DispatchRecordSubmittedAsync(int organizationId, int sourceAppId, EnvironmentStage stage, string dataJson, CancellationToken cancellationToken);
}
