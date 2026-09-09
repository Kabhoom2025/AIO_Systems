using FlowSphere.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FlowSphere.API.Hubs;

public class SignalRExecutionNotifier : IExecutionNotifier
{
    private readonly IHubContext<ExecutionHub> _hubContext;

    public SignalRExecutionNotifier(IHubContext<ExecutionHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyExecutionStartedAsync(int organizationId, Guid executionId, CancellationToken cancellationToken)
    {
        return Group(organizationId).SendAsync("ExecutionStarted", new { executionId }, cancellationToken);
    }

    public Task NotifyStepUpdatedAsync(int organizationId, Guid executionId, int stepLogId, string nodeKey, string status, CancellationToken cancellationToken)
    {
        return Group(organizationId).SendAsync("StepUpdated", new { executionId, stepLogId, nodeKey, status }, cancellationToken);
    }

    public Task NotifyExecutionCompletedAsync(int organizationId, Guid executionId, string status, CancellationToken cancellationToken)
    {
        return Group(organizationId).SendAsync("ExecutionCompleted", new { executionId, status }, cancellationToken);
    }

    private IClientProxy Group(int organizationId) => _hubContext.Clients.Group($"org-{organizationId}");
}
