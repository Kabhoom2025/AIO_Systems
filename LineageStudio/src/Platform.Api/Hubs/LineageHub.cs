using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Platform.Api.Hubs;

/// <summary>
/// Clients join one group per application (not per execution - the client doesn't know an
/// execution's id until after the HTTP call that creates it returns, by which point every event
/// for a fast execution has usually already fired) and receive every lineage event and execution
/// status change for that application in real time, no polling. Requires the same bearer token
/// as the REST API - see Program.cs's JwtBearerEvents.OnMessageReceived for how it arrives via
/// the connection's query string instead of a header (browsers can't set one on a WS upgrade).
/// </summary>
[Authorize]
public class LineageHub : Hub
{
    public static string ApplicationGroup(Guid applicationId) => $"app:{applicationId}";

    public Task JoinApplicationGroup(Guid applicationId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, ApplicationGroup(applicationId));

    public Task LeaveApplicationGroup(Guid applicationId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, ApplicationGroup(applicationId));
}
