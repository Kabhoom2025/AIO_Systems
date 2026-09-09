using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/shipments")]
[Authorize]
public class ShipmentController : ApiControllerBase
{
    private readonly IShipmentService _service;
    private readonly IShippingRateService _rateService;

    public ShipmentController(IShipmentService service, IShippingRateService rateService)
    {
        _service = service;
        _rateService = rateService;
    }

    [Authorize(Policy = "warehouse.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "warehouse.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "warehouse.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShipmentDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "warehouse.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateShipmentDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "warehouse.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "warehouse.edit")]
    [HttpPost("{id}/pick")]
    public async Task<IActionResult> Pick(int id) =>
        Ok(await _service.PickAsync(OrgId, id));

    [Authorize(Policy = "warehouse.edit")]
    [HttpPost("{id}/ship")]
    public async Task<IActionResult> Ship(int id) =>
        Ok(await _service.ShipAsync(OrgId, id));

    [Authorize(Policy = "warehouse.edit")]
    [HttpPost("{id}/deliver")]
    public async Task<IActionResult> Deliver(int id) =>
        Ok(await _service.DeliverAsync(OrgId, id));

    [Authorize(Policy = "warehouse.edit")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id) =>
        Ok(await _service.CancelAsync(OrgId, id));

    [Authorize(Policy = "warehouse.view")]
    [HttpGet("{id}/rates")]
    public async Task<IActionResult> GetRates(int id) =>
        Ok(await _rateService.GetRatesAsync(OrgId, id));
}
