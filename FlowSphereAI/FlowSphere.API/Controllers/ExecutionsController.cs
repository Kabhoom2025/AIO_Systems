using FlowSphere.Application.Executions.Commands.CompleteUserTask;
using FlowSphere.Application.Executions.Commands.SaveUserTaskDraft;
using FlowSphere.Application.Executions.Queries.GetExecutionDetail;
using FlowSphere.Application.Executions.Queries.GetPendingApprovals;
using FlowSphere.Domain.Common;
using FlowSphere.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[Authorize]
[Route("api/executions")]
public class ExecutionsController : ApiControllerBase
{
    [HttpGet("pending")]
    [Authorize(Policy = PermissionCatalog.WorkflowsApprove)]
    public async Task<IActionResult> GetPending(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? workflowDefinitionId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetPendingApprovalsQuery(page, pageSize, workflowDefinitionId), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.ExecutionsRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetExecutionDetailQuery(id), cancellationToken);
        return FromResult(result);
    }

    public record CompleteUserTaskRequest(bool Approved, string? Comment);

    [HttpPost("{id:guid}/complete-user-task")]
    [Authorize(Policy = PermissionCatalog.WorkflowsApprove)]
    public async Task<IActionResult> CompleteUserTask(Guid id, CompleteUserTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new CompleteUserTaskCommand(id, request.Approved, request.Comment), cancellationToken);
        return FromResult(result);
    }

    public record SaveDraftRequest(string DraftDataJson);

    [HttpPost("{id:guid}/save-draft")]
    [Authorize(Policy = PermissionCatalog.WorkflowsApprove)]
    public async Task<IActionResult> SaveDraft(Guid id, SaveDraftRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SaveUserTaskDraftCommand(id, request.DraftDataJson), cancellationToken);
        return FromResult(result);
    }
}
