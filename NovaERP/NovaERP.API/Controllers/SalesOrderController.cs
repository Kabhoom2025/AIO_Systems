using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/sales-orders")]
[Authorize]
public class SalesOrderController : ApiControllerBase
{
    private readonly ISalesOrderService _service;

    public SalesOrderController(ISalesOrderService service) => _service = service;

    [Authorize(Policy = "sales.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "sales.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "sales.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "sales.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSalesOrderDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "sales.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "sales.edit")]
    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id) =>
        Ok(await _service.ConfirmAsync(OrgId, id));

    [Authorize(Policy = "sales.edit")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id) =>
        Ok(await _service.CancelAsync(OrgId, id));
}
