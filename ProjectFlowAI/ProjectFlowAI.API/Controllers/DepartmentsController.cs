using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Departments;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartmentsController(IMediator mediator) => _mediator = mediator;

    public record CreateDepartmentRequest(Guid OrganizationId, string Name, string? Description, Guid? ParentDepartmentId);
    public record UpdateDepartmentRequest(string Name, string? Description, Guid? ParentDepartmentId);

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.DepartmentsView)]
    public async Task<ActionResult<PagedResult<DepartmentDto>>> List(
        [FromQuery] Guid organizationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = "asc", [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new ListDepartmentsQuery(organizationId, page, pageSize, sortBy, sortDir, search));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.DepartmentsManage)]
    public async Task<ActionResult<DepartmentDto>> Create(CreateDepartmentRequest request)
    {
        var result = await _mediator.Send(new CreateDepartmentCommand(
            request.OrganizationId, request.Name, request.Description, request.ParentDepartmentId));
        return CreatedAtAction(nameof(List), new { organizationId = request.OrganizationId }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.DepartmentsManage)]
    public async Task<ActionResult<DepartmentDto>> Update(Guid id, UpdateDepartmentRequest request)
    {
        var result = await _mediator.Send(new UpdateDepartmentCommand(id, request.Name, request.Description, request.ParentDepartmentId));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.DepartmentsManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteDepartmentCommand(id));
        return NoContent();
    }
}
