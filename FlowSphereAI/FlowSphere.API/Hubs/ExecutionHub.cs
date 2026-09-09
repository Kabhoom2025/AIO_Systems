using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FlowSphere.API.Hubs;

/// <summary>Clients join the group named "org-{organizationId}" (their own tenant's group,
/// read from their own JWT - never a caller-supplied id) after connecting, and receive
/// ExecutionStarted/StepUpdated/ExecutionCompleted pushes for anything happening in that
/// tenant.</summary>
[Authorize]
public class ExecutionHub : Hub
{
    public async Task JoinOrganizationGroup()
    {
        var organizationId = Context.User?.FindFirst("organizationId")?.Value;
        if (string.IsNullOrEmpty(organizationId))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"org-{organizationId}");
    }
}
