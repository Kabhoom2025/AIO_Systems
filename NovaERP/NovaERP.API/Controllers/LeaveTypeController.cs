using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/leave-types")]
[Authorize]
public class LeaveTypeController : ApiControllerBase
{
    private readonly ILeaveTypeService _service;

    public LeaveTypeController(ILeaveTypeService service) => _service = service;

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
    public async Task<IActionResult> Create([FromBody] CreateLeaveTypeDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "hrms.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLeaveTypeDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "hrms.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }
}
