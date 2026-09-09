using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/designations")]
[Authorize]
public class DesignationController : ApiControllerBase
{
    private readonly IDesignationService _service;

    public DesignationController(IDesignationService service) => _service = service;

    [Authorize(Policy = "designations.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "designations.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "designations.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDesignationDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "designations.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDesignationDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "designations.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "designations.view")]
    [HttpGet("job-grades")]
    public async Task<IActionResult> GetJobGrades() =>
        Ok(await _service.GetJobGradesAsync(OrgId));

    [Authorize(Policy = "designations.create")]
    [HttpPost("job-grades")]
    public async Task<IActionResult> CreateJobGrade([FromBody] CreateJobGradeDto dto) =>
        Ok(await _service.CreateJobGradeAsync(OrgId, dto));

    [Authorize(Policy = "designations.edit")]
    [HttpPut("job-grades/{id}")]
    public async Task<IActionResult> UpdateJobGrade(int id, [FromBody] UpdateJobGradeDto dto) =>
        Ok(await _service.UpdateJobGradeAsync(OrgId, id, dto));

    [Authorize(Policy = "designations.delete")]
    [HttpDelete("job-grades/{id}")]
    public async Task<IActionResult> DeleteJobGrade(int id)
    {
        await _service.DeleteJobGradeAsync(OrgId, id);
        return NoContent();
    }
}
