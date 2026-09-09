using Microsoft.AspNetCore.SignalR;
using Platform.Lineage.Recording;

namespace Platform.Api.Hubs;

/// <summary>
/// Replaces NullLineageEventPublisher (see Platform.Lineage.DependencyInjection) so every
/// event/execution update the Lineage Engine records also gets pushed live to whichever clients
/// are watching that application - no timers, no fake animation, just the real events as they're
/// recorded server-side.
/// </summary>
public class SignalRLineageEventPublisher : ILineageEventPublisher
{
    private readonly IHubContext<LineageHub> _hub;

    public SignalRLineageEventPublisher(IHubContext<LineageHub> hub)
    {
        _hub = hub;
    }

    public Task PublishEventAsync(LineageEventDto evt, CancellationToken ct = default) =>
        _hub.Clients.Group(LineageHub.ApplicationGroup(evt.ApplicationId)).SendAsync("lineageEvent", evt, ct);

    public Task PublishExecutionUpdateAsync(LineageExecutionDto execution, CancellationToken ct = default) =>
        _hub.Clients.Group(LineageHub.ApplicationGroup(execution.ApplicationId)).SendAsync("lineageExecutionUpdate", execution, ct);
}
