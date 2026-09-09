using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/holidays")]
[Authorize]
public class HolidayController : ApiControllerBase
{
    private readonly IHolidayService _service;

    public HolidayController(IHolidayService service) => _service = service;

    [Authorize(Policy = "holidays.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? year) =>
        Ok(await _service.GetAllAsync(OrgId, year));

    [Authorize(Policy = "holidays.view")]
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming() =>
        Ok(await _service.GetUpcomingAsync(OrgId));

    [Authorize(Policy = "holidays.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "holidays.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHolidayDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "holidays.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHolidayDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "holidays.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }
}
