using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;
using Workflow.Application.Services;

namespace Workflow.API.Controllers;

[Route("api/workflows")]
[Authorize]
public class WorkflowController : ApiControllerBase
{
    private readonly IWorkflowDesignService _service;

    public WorkflowController(IWorkflowDesignService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowDefinitionDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkflowDefinitionDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [HttpGet("{id}/versions")]
    public async Task<IActionResult> GetVersions(int id) =>
        Ok(await _service.GetVersionsAsync(OrgId, id));

    [HttpGet("{id}/draft")]
    public async Task<IActionResult> GetDraft(int id) =>
        Ok(await _service.GetDraftAsync(OrgId, id));

    [HttpPut("{id}/draft")]
    public async Task<IActionResult> SaveDraft(int id, [FromBody] SaveDraftGraphDto dto) =>
        Ok(await _service.SaveDraftAsync(OrgId, id, dto));

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(int id) =>
        Ok(await _service.PublishAsync(OrgId, id, UserId));

    [HttpGet("meta/triggers")]
    public IActionResult GetTriggerTypes() =>
        Ok(_service.GetTriggerTypes());

    [HttpGet("meta/actions")]
    public IActionResult GetActionTypes() =>
        Ok(_service.GetActionTypes());

    [HttpGet("{id}/executions")]
    public async Task<IActionResult> GetExecutions(int id) =>
        Ok(await _service.GetExecutionsAsync(OrgId, id));

    [HttpPost("{id}/run")]
    public async Task<IActionResult> Run(int id, [FromBody] RunWorkflowDto dto) =>
        Ok(await _service.RunDraftAsync(OrgId, id, dto));

    /// <summary>Same as Run, but streams each step live over Server-Sent Events as it happens
    /// instead of blocking for the whole run.</summary>
    [HttpPost("{id}/run-stream")]
    public async Task RunStream(int id, [FromBody] RunWorkflowDto dto)
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";

        async Task OnStep(WorkflowStepEvent evt)
        {
            await Response.WriteAsync($"data: {JsonSerializer.Serialize(evt)}\n\n");
            await Response.Body.FlushAsync();
        }

        var result = await _service.RunDraftAsync(OrgId, id, dto, OnStep);
        await Response.WriteAsync($"data: {JsonSerializer.Serialize(new { done = true, execution = result })}\n\n");
        await Response.Body.FlushAsync();
    }

    [AllowAnonymous]
    [HttpPost("webhook/{token}")]
    public async Task<IActionResult> Webhook(string token, [FromBody] RunWorkflowDto dto)
    {
        var result = await _service.RunByWebhookAsync(token, dto);
        if (result == null) return NotFound(new { message = "No published workflow found for this webhook." });
        return Ok(result);
    }
}
