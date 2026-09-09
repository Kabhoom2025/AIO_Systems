using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/workflows")]
[Authorize]
public class WorkflowController : ApiControllerBase
{
    private readonly IWorkflowDesignService _service;

    public WorkflowController(IWorkflowDesignService service) => _service = service;

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

    [Authorize(Policy = "workflows.view")]
    [HttpGet("{id}/versions")]
    public async Task<IActionResult> GetVersions(int id) =>
        Ok(await _service.GetVersionsAsync(OrgId, id));

    [Authorize(Policy = "workflows.view")]
    [HttpGet("{id}/draft")]
    public async Task<IActionResult> GetDraft(int id) =>
        Ok(await _service.GetDraftAsync(OrgId, id));

    [Authorize(Policy = "workflows.edit")]
    [HttpPut("{id}/draft")]
    public async Task<IActionResult> SaveDraft(int id, [FromBody] SaveDraftGraphDto dto) =>
        Ok(await _service.SaveDraftAsync(OrgId, id, dto));

    [Authorize(Policy = "workflows.edit")]
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(int id) =>
        Ok(await _service.PublishAsync(OrgId, id, UserId));

    [Authorize(Policy = "workflows.view")]
    [HttpGet("meta/triggers")]
    public IActionResult GetTriggerTypes() =>
        Ok(_service.GetTriggerTypes());

    [Authorize(Policy = "workflows.view")]
    [HttpGet("meta/actions")]
    public IActionResult GetActionTypes() =>
        Ok(_service.GetActionTypes());

    [Authorize(Policy = "workflows.view")]
    [HttpGet("{id}/executions")]
    public async Task<IActionResult> GetExecutions(int id) =>
        Ok(await _service.GetExecutionsAsync(OrgId, id));
}
