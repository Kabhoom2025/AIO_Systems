using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Organizations;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/organizations")]
[Authorize]
public class OrganizationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrganizationsController(IMediator mediator) => _mediator = mediator;

    public record CreateOrganizationRequest(string Name, string Slug, string? Domain, SubscriptionPlan SubscriptionPlan);
    public record UpdateOrganizationRequest(string Name, string? LogoUrl, string? Domain, SubscriptionPlan SubscriptionPlan, bool IsActive);

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.OrganizationsView)]
    public async Task<ActionResult<PagedResult<OrganizationDto>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = "asc", [FromQuery] string? search = null, [FromQuery] bool? isActive = null)
    {
        var result = await _mediator.Send(new ListOrganizationsQuery(page, pageSize, sortBy, sortDir, search, isActive));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.OrganizationsView)]
    public async Task<ActionResult<OrganizationDto>> Get(Guid id)
    {
        var result = await _mediator.Send(new GetOrganizationQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.OrganizationsManage)]
    public async Task<ActionResult<OrganizationDto>> Create(CreateOrganizationRequest request)
    {
        var result = await _mediator.Send(new CreateOrganizationCommand(request.Name, request.Slug, request.Domain, request.SubscriptionPlan));
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.OrganizationsManage)]
    public async Task<ActionResult<OrganizationDto>> Update(Guid id, UpdateOrganizationRequest request)
    {
        var result = await _mediator.Send(new UpdateOrganizationCommand(
            id, request.Name, request.LogoUrl, request.Domain, request.SubscriptionPlan, request.IsActive));
        return Ok(result);
    }
}
