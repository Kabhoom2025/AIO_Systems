using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/leave-requests")]
[Authorize]
public class LeaveRequestController : ApiControllerBase
{
    private readonly ILeaveRequestService _service;

    public LeaveRequestController(ILeaveRequestService service) => _service = service;

    [Authorize(Policy = "hrms.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "hrms.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "hrms.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "hrms.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLeaveRequestDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "hrms.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "hrms.edit")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id) =>
        Ok(await _service.ApproveAsync(OrgId, id));

    [Authorize(Policy = "hrms.edit")]
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id) =>
        Ok(await _service.RejectAsync(OrgId, id));

    [Authorize(Policy = "hrms.edit")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id) =>
        Ok(await _service.CancelAsync(OrgId, id));
}
