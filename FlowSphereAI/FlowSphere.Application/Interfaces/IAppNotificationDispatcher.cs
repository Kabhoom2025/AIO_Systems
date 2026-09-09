namespace FlowSphere.Application.Interfaces;

/// <summary>Evaluates and sends an app's configured notification rules for a just-created
/// AppRecord. Only Event=RecordSubmitted/Custom + Channel=Email actually dispatch - see
/// AppNotificationEvent/AppNotificationChannel for the honest scope cut.</summary>
public interface IAppNotificationDispatcher
{
    Task DispatchRecordSubmittedAsync(int organizationId, int appId, string dataJson, CancellationToken cancellationToken);
}
