using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Automation;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/automation")]
[Authorize]
public class AutomationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public AutomationController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new UnauthorizedDomainException("Authenticated request is missing a user id claim.");

    public record CreateWorkflowRequest(Guid OrganizationId, Guid? ProjectId, string Name, WorkflowTriggerTypeRequest TriggerType,
        string? CronExpression, IReadOnlyList<WorkflowConditionInput> Conditions, IReadOnlyList<WorkflowActionInput> Actions);
    public record UpdateWorkflowRequest(string Name, string? CronExpression,
        IReadOnlyList<WorkflowConditionInput> Conditions, IReadOnlyList<WorkflowActionInput> Actions);
    public record DecideApprovalRequest(bool Approve, string? Comment);

    // Alias so the request body's TriggerType binds the same way as the domain enum without
    // exposing an API.Controllers -> Domain leak concern — it IS the domain enum, just named for
    // clarity at the request-model boundary.
    public enum WorkflowTriggerTypeRequest
    {
        WorkItemCreated, WorkItemStatusChanged, WorkItemAssigned, SprintStarted, Scheduled
    }

    [HttpGet("workflows")]
    [Authorize(Policy = PermissionCatalog.AutomationManage)]
    public async Task<ActionResult<IReadOnlyList<WorkflowSummaryDto>>> ListWorkflows([FromQuery] Guid? projectId)
        => Ok(await _mediator.Send(new ListWorkflowsQuery(projectId)));

    [HttpPost("workflows")]
    [Authorize(Policy = PermissionCatalog.AutomationManage)]
    public async Task<ActionResult<WorkflowDetailDto>> CreateWorkflow(CreateWorkflowRequest request)
    {
        var result = await _mediator.Send(new CreateWorkflowCommand(request.OrganizationId, request.ProjectId, request.Name,
            (Domain.WorkflowTriggerType)request.TriggerType, request.CronExpression, request.Conditions, request.Actions, CurrentUserId));
        return CreatedAtAction(nameof(GetWorkflow), new { id = result.Id }, result);
    }

    [HttpGet("workflows/{id:guid}")]
    [Authorize(Policy = PermissionCatalog.AutomationManage)]
    public async Task<ActionResult<WorkflowDetailDto>> GetWorkflow(Guid id)
        => Ok(await _mediator.Send(new GetWorkflowQuery(id)));

    [HttpPut("workflows/{id:guid}")]
    [Authorize(Policy = PermissionCatalog.AutomationManage)]
    public async Task<ActionResult<WorkflowDetailDto>> UpdateWorkflow(Guid id, UpdateWorkflowRequest request)
        => Ok(await _mediator.Send(new UpdateWorkflowCommand(id, request.Name, request.CronExpression, request.Conditions, request.Actions)));

    [HttpDelete("workflows/{id:guid}")]
    [Authorize(Policy = PermissionCatalog.AutomationManage)]
    public async Task<IActionResult> DeleteWorkflow(Guid id)
    {
        await _mediator.Send(new DeleteWorkflowCommand(id));
        return NoContent();
    }

    [HttpPost("workflows/{id:guid}/enable")]
    [Authorize(Policy = PermissionCatalog.AutomationManage)]
    public async Task<IActionResult> EnableWorkflow(Guid id)
    {
        await _mediator.Send(new EnableWorkflowCommand(id));
        return NoContent();
    }

    [HttpPost("workflows/{id:guid}/disable")]
    [Authorize(Policy = PermissionCatalog.AutomationManage)]
    public async Task<IActionResult> DisableWorkflow(Guid id)
    {
        await _mediator.Send(new DisableWorkflowCommand(id));
        return NoContent();
    }

    [HttpGet("workflows/{id:guid}/runs")]
    [Authorize(Policy = PermissionCatalog.AutomationManage)]
    public async Task<ActionResult<PagedResult<WorkflowRunDto>>> ListRuns(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _mediator.Send(new ListWorkflowRunsQuery(id, page, pageSize)));

    // Approvals are intentionally NOT gated by PermissionCatalog.AutomationManage — any
    // authenticated user can view/decide the approvals assigned to them; the handler enforces
    // "you are the RequestedApproverUserId on this specific approval".
    [HttpGet("approvals")]
    public async Task<ActionResult<IReadOnlyList<ApprovalSummaryDto>>> ListApprovals([FromQuery] bool assignedToMe = true)
        => Ok(await _mediator.Send(new ListApprovalsQuery(CurrentUserId, assignedToMe)));

    [HttpPost("approvals/{id:guid}/decide")]
    public async Task<IActionResult> DecideApproval(Guid id, DecideApprovalRequest request)
    {
        await _mediator.Send(new DecideApprovalCommand(id, request.Approve, request.Comment, CurrentUserId));
        return NoContent();
    }
}
