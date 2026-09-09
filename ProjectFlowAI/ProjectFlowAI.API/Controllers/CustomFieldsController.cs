using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.CustomFields;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/custom-fields")]
[Authorize]
public class CustomFieldsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomFieldsController(IMediator mediator) => _mediator = mediator;

    public record CreateCustomFieldDefinitionRequest(Guid ProjectId, string Name, CustomFieldType FieldType, string? OptionsJson, bool IsRequired);

    [HttpGet]
    [Authorize(Policy = PermissionCatalog.ProjectsView)]
    public async Task<ActionResult<IReadOnlyList<CustomFieldDefinitionDto>>> List([FromQuery] Guid projectId)
    {
        var result = await _mediator.Send(new ListCustomFieldDefinitionsQuery(projectId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.ProjectsManage)]
    public async Task<ActionResult<CustomFieldDefinitionDto>> Create(CreateCustomFieldDefinitionRequest request)
    {
        var result = await _mediator.Send(new CreateCustomFieldDefinitionCommand(
            request.ProjectId, request.Name, request.FieldType, request.OptionsJson, request.IsRequired));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCatalog.ProjectsManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteCustomFieldDefinitionCommand(id));
        return NoContent();
    }
}
