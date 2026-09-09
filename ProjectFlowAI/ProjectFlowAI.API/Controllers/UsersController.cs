using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Users;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.UsersView)]
    public async Task<ActionResult<PagedResult<UserDto>>> List(
        [FromQuery] Guid? organizationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = "asc", [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new ListUsersQuery(organizationId, page, pageSize, sortBy, sortDir, search));
        return Ok(result);
    }
}
