using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/stock-transfers")]
[Authorize]
public class StockTransferController : ApiControllerBase
{
    private readonly IStockTransferService _service;

    public StockTransferController(IStockTransferService service) => _service = service;

    [Authorize(Policy = "warehouse.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "warehouse.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockTransferDto dto) =>
        Ok(await _service.CreateAsync(OrgId, dto));
}
