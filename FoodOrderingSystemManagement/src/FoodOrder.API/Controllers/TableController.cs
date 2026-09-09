using FoodOrder.Application.DTOs.Table;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class TableController : BaseApiController
{
    private readonly ITableService _tableService;

    public TableController(ITableService tableService)
    {
        _tableService = tableService;
    }

    /// <summary>Get all tables with their current occupied status (Admin + Cashier).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TableDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var tables = await _tableService.GetAllAsync();
        return Ok(ApiResponse<IReadOnlyList<TableDto>>.SuccessResult(tables));
    }

    /// <summary>Get a single table by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TableDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var table = await _tableService.GetByIdAsync(id);
        return Ok(ApiResponse<TableDto>.SuccessResult(table));
    }

    /// <summary>Create a new table (Admin only).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<TableDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTableDto dto)
    {
        var table = await _tableService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = table.Id },
            ApiResponse<TableDto>.SuccessResult(table, "Table created successfully."));
    }

    /// <summary>Update a table (Admin only).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<TableDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTableDto dto)
    {
        var table = await _tableService.UpdateAsync(id, dto);
        return Ok(ApiResponse<TableDto>.SuccessResult(table, "Table updated successfully."));
    }

    /// <summary>Soft-delete a table (Admin only).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id)
    {
        await _tableService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Table deleted successfully."));
    }
}
