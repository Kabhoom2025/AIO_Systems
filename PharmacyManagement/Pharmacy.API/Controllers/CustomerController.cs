using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _service;

    public CustomerController(ICustomerService service) => _service = service;

    [Authorize(Policy = "customers.view")]
    [HttpGet("org/{orgId}")]
    public async Task<IActionResult> GetAll(int orgId) =>
        Ok(await _service.GetAllAsync(orgId));

    [Authorize(Policy = "customers.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Policy = "customers.create")]
    [HttpPost("org/{orgId}")]
    public async Task<IActionResult> Create(int orgId, [FromBody] CreateCustomerDto dto)
    {
        var result = await _service.CreateAsync(orgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "customers.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto) =>
        Ok(await _service.UpdateAsync(id, dto));

    [Authorize(Policy = "customers.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
