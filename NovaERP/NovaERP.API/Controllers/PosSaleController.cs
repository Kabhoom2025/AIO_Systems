using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/pos-sales")]
[Authorize]
public class PosSaleController : ApiControllerBase
{
    private readonly IPosSaleService _service;

    public PosSaleController(IPosSaleService service) => _service = service;

    [Authorize(Policy = "pos.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "pos.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "pos.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePosSaleDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "pos.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePosSaleDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "pos.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "pos.edit")]
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] CompletePosSaleDto dto) =>
        Ok(await _service.CompleteAsync(OrgId, id, dto));

    [Authorize(Policy = "pos.edit")]
    [HttpPost("{id}/refund")]
    public async Task<IActionResult> Refund(int id) =>
        Ok(await _service.RefundAsync(OrgId, id));

    [Authorize(Policy = "pos.edit")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id) =>
        Ok(await _service.CancelAsync(OrgId, id));
}
