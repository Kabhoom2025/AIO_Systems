using FoodOrder.Application.DTOs;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodOrder.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Admin,Cashier")]
public class LedgerController : BaseApiController
{
    private readonly ILedgerService _ledgerService;

    public LedgerController(ILedgerService ledgerService)
    {
        _ledgerService = ledgerService;
    }

    /// <summary>Get ledger entries for a date range (defaults to today).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LedgerEntryDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntries(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var start = from?.Date ?? DateTime.UtcNow.Date;
        var end   = to?.Date   ?? DateTime.UtcNow.Date;
        var entries = await _ledgerService.GetEntriesAsync(start, end);
        return Ok(ApiResponse<IReadOnlyList<LedgerEntryDTO>>.SuccessResult(entries));
    }

    /// <summary>Get daily summary (credit, debit, net) for a given date.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<LedgerSummaryDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? date)
    {
        var summary = await _ledgerService.GetDailySummaryAsync(date?.Date ?? DateTime.UtcNow.Date);
        return Ok(ApiResponse<LedgerSummaryDTO>.SuccessResult(summary));
    }

    /// <summary>Create a new ledger entry.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LedgerEntryDTO>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateLedgerEntryRequest dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var entry  = await _ledgerService.CreateAsync(dto, userId);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<LedgerEntryDTO>.SuccessResult(entry, "Entry added successfully."));
    }

    /// <summary>Delete a ledger entry (Admin only).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id)
    {
        await _ledgerService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null!, "Entry deleted."));
    }
}
