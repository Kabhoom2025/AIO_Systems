using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseService _service;

    public ExpenseController(IExpenseService service) => _service = service;

    [Authorize(Policy = "expenses.view")]
    [HttpGet("org/{orgId}")]
    public async Task<IActionResult> GetAll(int orgId) =>
        Ok(await _service.GetAllAsync(orgId));

    [Authorize(Policy = "expenses.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Policy = "expenses.create")]
    [HttpPost("org/{orgId}")]
    public async Task<IActionResult> Create(int orgId, [FromBody] CreateExpenseDto dto)
    {
        var result = await _service.CreateAsync(orgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "expenses.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseDto dto) =>
        Ok(await _service.UpdateAsync(id, dto));

    [Authorize(Policy = "expenses.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
