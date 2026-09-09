using FoodOrder.Application.DTOs;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Admin,InventoryManager")]
public class InventoryController : BaseApiController
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _inventoryService.GetAllAsync();
        return Ok(ApiResponse<IReadOnlyList<InventoryItemDTO>>.SuccessResult(items));
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var items = await _inventoryService.GetLowStockAsync();
        return Ok(ApiResponse<IReadOnlyList<InventoryItemDTO>>.SuccessResult(items));
    }

    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiring([FromQuery] int days = 7)
    {
        var items = await _inventoryService.GetExpiringAsync(days);
        return Ok(ApiResponse<IReadOnlyList<InventoryItemDTO>>.SuccessResult(items));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _inventoryService.GetByIdAsync(id);
        return Ok(ApiResponse<InventoryItemDTO>.SuccessResult(item));
    }

    [HttpGet("barcode/{code}")]
    public async Task<IActionResult> GetByBarcode(string code)
    {
        var item = await _inventoryService.GetByBarcodeAsync(code);
        if (item is null)
            return NotFound(ApiResponse<object>.FailureResult($"No inventory item found for barcode '{code}'."));
        return Ok(ApiResponse<InventoryItemDTO>.SuccessResult(item));
    }

    [HttpGet("{id:int}/transactions")]
    public async Task<IActionResult> GetTransactions(int id)
    {
        var txs = await _inventoryService.GetTransactionsAsync(id);
        return Ok(ApiResponse<IReadOnlyList<StockTransactionDTO>>.SuccessResult(txs));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemRequest dto)
    {
        var item = await _inventoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            ApiResponse<InventoryItemDTO>.SuccessResult(item, "Inventory item created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInventoryItemRequest dto)
    {
        var item = await _inventoryService.UpdateAsync(id, dto);
        return Ok(ApiResponse<InventoryItemDTO>.SuccessResult(item, "Inventory item updated successfully."));
    }

    [HttpPatch("{id:int}/adjust")]
    public async Task<IActionResult> AdjustStock(int id, [FromBody] StockAdjustmentRequest dto)
    {
        var item = await _inventoryService.AdjustStockAsync(id, dto);
        return Ok(ApiResponse<InventoryItemDTO>.SuccessResult(item, "Stock adjusted successfully."));
    }

    [HttpPost("{id:int}/purchase")]
    public async Task<IActionResult> PurchaseStock(int id, [FromBody] PurchaseStockRequest dto)
    {
        var item = await _inventoryService.PurchaseStockAsync(id, dto);
        return Ok(ApiResponse<InventoryItemDTO>.SuccessResult(item, "Stock purchase recorded."));
    }

    [HttpPost("{id:int}/waste")]
    public async Task<IActionResult> RecordWaste(int id, [FromBody] WasteStockRequest dto)
    {
        var item = await _inventoryService.WasteStockAsync(id, dto);
        return Ok(ApiResponse<InventoryItemDTO>.SuccessResult(item, "Waste recorded successfully."));
    }

    [HttpPost("{id:int}/transfer")]
    public async Task<IActionResult> TransferStock(int id, [FromBody] TransferStockRequest dto)
    {
        var (source, target) = await _inventoryService.TransferStockAsync(id, dto);
        return Ok(ApiResponse<object>.SuccessResult(new { source, target }, "Stock transferred successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _inventoryService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null!, "Inventory item deleted successfully."));
    }
}
