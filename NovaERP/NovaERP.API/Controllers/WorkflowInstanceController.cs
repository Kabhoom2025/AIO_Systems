using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/workflow-instances")]
[Authorize]
public class WorkflowInstanceController : ApiControllerBase
{
    private readonly IWorkflowInstanceService _service;

    public WorkflowInstanceController(IWorkflowInstanceService service) => _service = service;

    [Authorize(Policy = "workflows.create")]
    [HttpPost]
    public async Task<IActionResult> Start([FromBody] StartWorkflowDto dto)
    {
        var result = await _service.StartAsync(OrgId, UserId, dto);
        return CreatedAtAction(nameof(GetByEntity), new { entityType = result.EntityType, entityId = result.EntityId }, result);
    }

    [Authorize(Policy = "workflows.view")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending() =>
        Ok(await _service.GetPendingForUserAsync(OrgId, UserId));

    [Authorize(Policy = "workflows.view")]
    [HttpGet("by-entity")]
    public async Task<IActionResult> GetByEntity([FromQuery] string entityType, [FromQuery] int entityId) =>
        Ok(await _service.GetByEntityAsync(OrgId, entityType, entityId));

    // Approve/reject reuse the "workflows.edit" policy rather than a dedicated "workflows.approve"
    // key — see report/PermissionCatalog notes: keeps the catalog's uniform module x action shape
    // (view/create/edit/delete for every module) instead of special-casing one module.
    [Authorize(Policy = "workflows.edit")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] WorkflowActionDto dto) =>
        Ok(await _service.ApproveAsync(OrgId, id, UserId, dto));

    [Authorize(Policy = "workflows.edit")]
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] WorkflowActionDto dto) =>
        Ok(await _service.RejectAsync(OrgId, id, UserId, dto));
}
