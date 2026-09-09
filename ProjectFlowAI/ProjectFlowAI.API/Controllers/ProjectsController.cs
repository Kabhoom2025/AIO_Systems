using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Projects;
using ProjectFlowAI.Application.Features.Sprints;
using ProjectFlowAI.Application.Features.WorkItems;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator) => _mediator = mediator;

    public record CreateProjectRequest(Guid OrganizationId, string Key, string Name, string? Description,
        Guid OwnerUserId, DateTime? StartDate, DateTime? EndDate);
    public record UpdateProjectRequest(string Name, string? Description, ProjectStatus Status,
        DateTime? StartDate, DateTime? EndDate, Guid OwnerUserId);
    public record AddProjectMemberRequest(Guid UserId, string RoleInProject);

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.ProjectsView)]
    public async Task<ActionResult<PagedResult<ProjectDto>>> List(
        [FromQuery] Guid organizationId, [FromQuery] ProjectStatus? status, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = "asc")
    {
        var result = await _mediator.Send(new ListProjectsQuery(organizationId, status, search, page, pageSize, sortBy, sortDir));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.ProjectsView)]
    public async Task<ActionResult<ProjectDto>> Get(Guid id)
    {
        var result = await _mediator.Send(new GetProjectQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:guid}/kanban")]
    [Authorize(Policy = PermissionCatalog.TasksView)]
    public async Task<ActionResult<KanbanBoardDto>> Kanban(Guid id)
    {
        var result = await _mediator.Send(new GetKanbanBoardQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:guid}/members")]
    [Authorize(Policy = PermissionCatalog.ProjectsView)]
    public async Task<ActionResult<IReadOnlyList<ProjectMemberDto>>> Members(Guid id)
    {
        var result = await _mediator.Send(new ListProjectMembersQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:guid}/velocity")]
    [Authorize(Policy = PermissionCatalog.SprintsView)]
    public async Task<ActionResult<VelocityDto>> Velocity(Guid id)
    {
        var result = await _mediator.Send(new GetProjectVelocityQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.ProjectsManage)]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectRequest request)
    {
        var result = await _mediator.Send(new CreateProjectCommand(request.OrganizationId, request.Key, request.Name,
            request.Description, request.OwnerUserId, request.StartDate, request.EndDate));
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.ProjectsManage)]
    public async Task<ActionResult<ProjectDto>> Update(Guid id, UpdateProjectRequest request)
    {
        var result = await _mediator.Send(new UpdateProjectCommand(id, request.Name, request.Description,
            request.Status, request.StartDate, request.EndDate, request.OwnerUserId));
        return Ok(result);
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = PermissionCatalog.ProjectsManage)]
    public async Task<IActionResult> Archive(Guid id)
    {
        await _mediator.Send(new ArchiveProjectCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/members")]
    [Authorize(Policy = PermissionCatalog.ProjectsManage)]
    public async Task<ActionResult<ProjectMemberDto>> AddMember(Guid id, AddProjectMemberRequest request)
    {
        var result = await _mediator.Send(new AddProjectMemberCommand(id, request.UserId, request.RoleInProject));
        return Ok(result);
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [Authorize(Policy = PermissionCatalog.ProjectsManage)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        await _mediator.Send(new RemoveProjectMemberCommand(id, userId));
        return NoContent();
    }
}
