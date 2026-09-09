using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/delivery-loads")]
[Authorize]
public class DeliveryLoadController : ApiControllerBase
{
    private readonly IDeliveryLoadService _service;

    public DeliveryLoadController(IDeliveryLoadService service) => _service = service;

    [Authorize(Policy = "logistics.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "logistics.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "logistics.view")]
    [HttpGet("available-shipments")]
    public async Task<IActionResult> GetAvailableShipments([FromQuery] int warehouseId) =>
        Ok(await _service.GetAvailableShipmentsAsync(OrgId, warehouseId));

    [Authorize(Policy = "logistics.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDeliveryLoadDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "logistics.edit")]
    [HttpPost("{id}/assign-shipment")]
    public async Task<IActionResult> AssignShipment(int id, [FromBody] AssignShipmentDto dto) =>
        Ok(await _service.AssignShipmentAsync(OrgId, id, dto));

    [Authorize(Policy = "logistics.edit")]
    [HttpPost("{id}/unassign-shipment/{shipmentId}")]
    public async Task<IActionResult> UnassignShipment(int id, int shipmentId) =>
        Ok(await _service.UnassignShipmentAsync(OrgId, id, shipmentId));

    [Authorize(Policy = "logistics.edit")]
    [HttpPost("{id}/dispatch")]
    public async Task<IActionResult> Dispatch(int id) =>
        Ok(await _service.DispatchAsync(OrgId, id));
}
