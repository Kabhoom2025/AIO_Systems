using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/production-orders")]
[Authorize]
public class ProductionOrderController : ApiControllerBase
{
    private readonly IProductionOrderService _service;

    public ProductionOrderController(IProductionOrderService service) => _service = service;

    [Authorize(Policy = "manufacturing.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "manufacturing.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "manufacturing.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionOrderDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "manufacturing.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductionOrderDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "manufacturing.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "manufacturing.edit")]
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(int id) =>
        Ok(await _service.CompleteAsync(OrgId, id));

    [Authorize(Policy = "manufacturing.edit")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id) =>
        Ok(await _service.CancelAsync(OrgId, id));
}
