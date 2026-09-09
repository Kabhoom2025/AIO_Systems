using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/workflow-definitions")]
[Authorize]
public class WorkflowDefinitionController : ApiControllerBase
{
    private readonly IWorkflowDefinitionService _service;

    public WorkflowDefinitionController(IWorkflowDefinitionService service) => _service = service;

    [Authorize(Policy = "workflows.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "workflows.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "workflows.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowDefinitionDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "workflows.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkflowDefinitionDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "workflows.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }
}
