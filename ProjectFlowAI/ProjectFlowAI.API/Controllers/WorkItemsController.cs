using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.CustomFields;
using ProjectFlowAI.Application.Features.WorkItems;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/work-items")]
[Authorize]
public class WorkItemsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public WorkItemsController(IMediator mediator, IFileStorageService fileStorage, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    // "Who is performing this action" is taken from the authenticated JWT (ICurrentUserService),
    // never from a client-supplied body/form field — otherwise any caller could attribute a
    // comment, time log, attachment, or status change to an arbitrary other user id. [Authorize]
    // on every action below guarantees a valid NameIdentifier claim, so this never returns null.
    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new UnauthorizedDomainException("Authenticated request is missing a user id claim.");

    public record CreateWorkItemRequest(Guid ProjectId, Guid? ParentWorkItemId, string Title, string? Description,
        WorkItemPriority Priority, WorkItemType Type, int? StoryPoints, decimal? EstimatedHours,
        Guid? AssigneeUserId, Guid? ReporterUserId, DateTime? DueDate, bool IsRecurring, int? RecurrenceIntervalDays);

    public record UpdateWorkItemRequest(string Title, string? Description, WorkItemPriority Priority,
        WorkItemType Type, int? StoryPoints, decimal? EstimatedHours, Guid? AssigneeUserId, DateTime? DueDate,
        bool IsRecurring, int? RecurrenceIntervalDays);

    public record MoveWorkItemRequest(WorkItemStatus NewStatus, double NewPosition);
    public record AddChecklistItemRequest(string Text);
    public record AddLabelRequest(Guid LabelId);
    public record AddFollowerRequest(Guid UserId);
    public record AddDependencyRequest(Guid DependsOnWorkItemId, WorkItemDependencyType DependencyType);
    public record AddCommentRequest(string Body, IReadOnlyList<Guid>? MentionedUserIds);
    public record UpdateCommentRequest(string Body);
    public record LogTimeRequest(int Minutes, string? Note, DateOnly LoggedDate, bool IsBillable = true);
    public record SetCustomFieldValueRequest(Guid CustomFieldDefinitionId, string ValueJson);
    public record MoveToSprintRequest(Guid? SprintId);
    public record SetTimeLogBillableRequest(bool IsBillable);

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.TasksView)]
    public async Task<ActionResult<PagedResult<WorkItemDto>>> List(
        [FromQuery] Guid projectId, [FromQuery] WorkItemStatus? status, [FromQuery] Guid? assigneeId,
        [FromQuery] WorkItemPriority? priority, [FromQuery] Guid? labelId, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = "asc")
    {
        var result = await _mediator.Send(new ListWorkItemsQuery(projectId, status, assigneeId, priority, labelId, search, page, pageSize, sortBy, sortDir));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.TasksView)]
    public async Task<ActionResult<WorkItemDetailDto>> Get(Guid id)
    {
        var result = await _mediator.Send(new GetWorkItemQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<ActionResult<WorkItemDto>> Create(CreateWorkItemRequest request)
    {
        var result = await _mediator.Send(new CreateWorkItemCommand(request.ProjectId, request.ParentWorkItemId,
            request.Title, request.Description, request.Priority, request.Type, request.StoryPoints,
            request.EstimatedHours, request.AssigneeUserId, request.ReporterUserId ?? CurrentUserId, request.DueDate,
            request.IsRecurring, request.RecurrenceIntervalDays));
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<ActionResult<WorkItemDto>> Update(Guid id, UpdateWorkItemRequest request)
    {
        var result = await _mediator.Send(new UpdateWorkItemCommand(id, request.Title, request.Description,
            request.Priority, request.Type, request.StoryPoints, request.EstimatedHours, request.AssigneeUserId,
            request.DueDate, request.IsRecurring, request.RecurrenceIntervalDays, CurrentUserId));
        return Ok(result);
    }

    [HttpPost("{id:guid}/move")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<ActionResult<WorkItemDto>> Move(Guid id, MoveWorkItemRequest request)
    {
        var result = await _mediator.Send(new MoveWorkItemCommand(id, request.NewStatus, request.NewPosition, CurrentUserId));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteWorkItemCommand(id));
        return NoContent();
    }

    // --- Checklist ---

    [HttpPost("{id:guid}/checklist")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<ActionResult<ChecklistItemDto>> AddChecklistItem(Guid id, AddChecklistItemRequest request)
    {
        var result = await _mediator.Send(new AddChecklistItemCommand(id, request.Text));
        return Ok(result);
    }

    [HttpPatch("{id:guid}/checklist/{itemId:guid}")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<ActionResult<ChecklistItemDto>> ToggleChecklistItem(Guid id, Guid itemId)
    {
        var result = await _mediator.Send(new ToggleChecklistItemCommand(id, itemId));
        return Ok(result);
    }

    [HttpDelete("{id:guid}/checklist/{itemId:guid}")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<IActionResult> DeleteChecklistItem(Guid id, Guid itemId)
    {
        await _mediator.Send(new DeleteChecklistItemCommand(id, itemId));
        return NoContent();
    }

    // --- Labels ---

    [HttpPost("{id:guid}/labels")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<IActionResult> AddLabel(Guid id, AddLabelRequest request)
    {
        await _mediator.Send(new AddWorkItemLabelCommand(id, request.LabelId));
        return NoContent();
    }

    [HttpDelete("{id:guid}/labels/{labelId:guid}")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<IActionResult> RemoveLabel(Guid id, Guid labelId)
    {
        await _mediator.Send(new RemoveWorkItemLabelCommand(id, labelId));
        return NoContent();
    }

    // --- Followers ---

    [HttpPost("{id:guid}/followers")]
    [Authorize(Policy = PermissionCatalog.TasksView)]
    public async Task<IActionResult> AddFollower(Guid id, AddFollowerRequest request)
    {
        await _mediator.Send(new AddWorkItemFollowerCommand(id, request.UserId));
        return NoContent();
    }

    [HttpDelete("{id:guid}/followers/{userId:guid}")]
    [Authorize(Policy = PermissionCatalog.TasksView)]
    public async Task<IActionResult> RemoveFollower(Guid id, Guid userId)
    {
        await _mediator.Send(new RemoveWorkItemFollowerCommand(id, userId));
        return NoContent();
    }

    // --- Dependencies ---

    [HttpPost("{id:guid}/dependencies")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<ActionResult<WorkItemDependencyDto>> AddDependency(Guid id, AddDependencyRequest request)
    {
        var result = await _mediator.Send(new AddWorkItemDependencyCommand(id, request.DependsOnWorkItemId, request.DependencyType));
        return Ok(result);
    }

    [HttpDelete("dependencies/{dependencyId:guid}")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<IActionResult> RemoveDependency(Guid dependencyId)
    {
        await _mediator.Send(new RemoveWorkItemDependencyCommand(dependencyId));
        return NoContent();
    }

    // --- Comments ---

    [HttpPost("{id:guid}/comments")]
    [Authorize(Policy = PermissionCatalog.CommentsManage)]
    public async Task<ActionResult<WorkItemCommentDto>> AddComment(Guid id, AddCommentRequest request)
    {
        var result = await _mediator.Send(new AddWorkItemCommentCommand(id, CurrentUserId, request.Body, request.MentionedUserIds ?? Array.Empty<Guid>()));
        return Ok(result);
    }

    [HttpPut("comments/{commentId:guid}")]
    [Authorize(Policy = PermissionCatalog.CommentsManage)]
    public async Task<ActionResult<WorkItemCommentDto>> UpdateComment(Guid commentId, UpdateCommentRequest request)
    {
        var result = await _mediator.Send(new UpdateWorkItemCommentCommand(commentId, request.Body));
        return Ok(result);
    }

    [HttpDelete("comments/{commentId:guid}")]
    [Authorize(Policy = PermissionCatalog.CommentsManage)]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        await _mediator.Send(new DeleteWorkItemCommentCommand(commentId, CurrentUserId));
        return NoContent();
    }

    // --- Attachments ---

    [HttpPost("{id:guid}/attachments")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<WorkItemAttachmentDto>> UploadAttachment(Guid id, [FromForm] IFormFile file)
    {
        if (file.Length == 0) return BadRequest("File is empty.");

        await using var stream = file.OpenReadStream();
        var stored = await _fileStorage.SaveAsync(stream, file.FileName);

        var result = await _mediator.Send(new AddWorkItemAttachmentCommand(id, file.FileName, stored.RelativePath, stored.SizeBytes, CurrentUserId));
        return Ok(result);
    }

    [HttpDelete("attachments/{attachmentId:guid}")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<IActionResult> DeleteAttachment(Guid attachmentId)
    {
        var relativePath = await _mediator.Send(new DeleteWorkItemAttachmentCommand(attachmentId));
        _fileStorage.Delete(relativePath);
        return NoContent();
    }

    // --- Time logs ---

    [HttpPost("{id:guid}/time-logs")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<ActionResult<WorkItemTimeLogDto>> LogTime(Guid id, LogTimeRequest request)
    {
        var result = await _mediator.Send(new LogWorkItemTimeCommand(id, CurrentUserId, request.Minutes, request.Note, request.LoggedDate, request.IsBillable));
        return Ok(result);
    }

    [HttpDelete("time-logs/{timeLogId:guid}")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<IActionResult> DeleteTimeLog(Guid timeLogId)
    {
        await _mediator.Send(new DeleteWorkItemTimeLogCommand(timeLogId));
        return NoContent();
    }

    [HttpPatch("time-logs/{timeLogId:guid}/billable")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<ActionResult<WorkItemTimeLogDto>> SetTimeLogBillable(Guid timeLogId, SetTimeLogBillableRequest request)
    {
        var result = await _mediator.Send(new SetTimeLogBillableCommand(timeLogId, request.IsBillable));
        return Ok(result);
    }

    // --- Sprint assignment (Scrum) ---

    [HttpPost("{id:guid}/sprint")]
    [Authorize(Policy = PermissionCatalog.SprintsManage)]
    public async Task<ActionResult<WorkItemDto>> MoveToSprint(Guid id, MoveToSprintRequest request)
    {
        var result = await _mediator.Send(new MoveWorkItemToSprintCommand(id, request.SprintId));
        return Ok(result);
    }

    // --- Custom field values ---

    [HttpPost("{id:guid}/custom-field-values")]
    [Authorize(Policy = PermissionCatalog.TasksManage)]
    public async Task<ActionResult<CustomFieldValueDto>> SetCustomFieldValue(Guid id, SetCustomFieldValueRequest request)
    {
        var result = await _mediator.Send(new SetCustomFieldValueCommand(id, request.CustomFieldDefinitionId, request.ValueJson));
        return Ok(result);
    }
}
