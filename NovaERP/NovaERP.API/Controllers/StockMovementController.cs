using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/stock-movements")]
[Authorize]
public class StockMovementController : ApiControllerBase
{
    private readonly IStockMovementService _service;

    public StockMovementController(IStockMovementService service) => _service = service;

    [Authorize(Policy = "inventory.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? productId) =>
        Ok(await _service.GetAllAsync(OrgId, productId));

    [Authorize(Policy = "inventory.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockMovementDto dto) =>
        Ok(await _service.CreateAsync(OrgId, dto));

    [Authorize(Policy = "inventory.view")]
    [HttpGet("on-hand")]
    public async Task<IActionResult> GetOnHand([FromQuery] int productId, [FromQuery] int warehouseId) =>
        Ok(await _service.GetOnHandAtWarehouseAsync(OrgId, productId, warehouseId));
}
