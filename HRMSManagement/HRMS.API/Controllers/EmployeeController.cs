using System.Text;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/employees")]
[Authorize]
public class EmployeeController : ApiControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeeController(IEmployeeService service) => _service = service;

    [Authorize(Policy = "employees.view")]
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] int? branchId = null,
        [FromQuery] string? status = null) =>
        Ok(await _service.GetPagedAsync(OrgId, page, pageSize, search, departmentId, branchId, status));

    [Authorize(Policy = "employees.view")]
    [HttpGet("lookup")]
    public async Task<IActionResult> GetLookup() =>
        Ok(await _service.GetLookupAsync(OrgId));

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        if (EmployeeId is null) return NotFound();
        return Ok(await _service.GetMyProfileAsync(OrgId, EmployeeId.Value));
    }

    [Authorize(Policy = "employees.view")]
    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var csv = await _service.ExportCsvAsync(OrgId);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "employees.csv");
    }

    [Authorize(Policy = "employees.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "employees.view")]
    [HttpGet("{id}/team")]
    public async Task<IActionResult> GetTeam(int id) =>
        Ok(await _service.GetTeamAsync(OrgId, id));

    [Authorize(Policy = "employees.view")]
    [HttpGet("{id}/lifecycle-events")]
    public async Task<IActionResult> GetLifecycleEvents(int id) =>
        Ok(await _service.GetLifecycleEventsAsync(OrgId, id));

    [Authorize(Policy = "employees.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "employees.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "employees.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "employees.edit")]
    [HttpPost("{id}/documents")]
    public async Task<IActionResult> AddDocument(int id, [FromBody] CreateEmployeeDocumentDto dto) =>
        Ok(await _service.AddDocumentAsync(OrgId, id, dto));

    [Authorize(Policy = "employees.edit")]
    [HttpDelete("{id}/documents/{docId}")]
    public async Task<IActionResult> RemoveDocument(int id, int docId)
    {
        await _service.RemoveDocumentAsync(OrgId, id, docId);
        return NoContent();
    }

    [Authorize(Policy = "employees.edit")]
    [HttpPost("{id}/educations")]
    public async Task<IActionResult> AddEducation(int id, [FromBody] CreateEmployeeEducationDto dto) =>
        Ok(await _service.AddEducationAsync(OrgId, id, dto));

    [Authorize(Policy = "employees.edit")]
    [HttpDelete("{id}/educations/{eduId}")]
    public async Task<IActionResult> RemoveEducation(int id, int eduId)
    {
        await _service.RemoveEducationAsync(OrgId, id, eduId);
        return NoContent();
    }

    [Authorize(Policy = "employees.edit")]
    [HttpPost("{id}/experiences")]
    public async Task<IActionResult> AddExperience(int id, [FromBody] CreateEmployeeExperienceDto dto) =>
        Ok(await _service.AddExperienceAsync(OrgId, id, dto));

    [Authorize(Policy = "employees.edit")]
    [HttpDelete("{id}/experiences/{expId}")]
    public async Task<IActionResult> RemoveExperience(int id, int expId)
    {
        await _service.RemoveExperienceAsync(OrgId, id, expId);
        return NoContent();
    }

    [Authorize(Policy = "employees.edit")]
    [HttpPost("{id}/family-members")]
    public async Task<IActionResult> AddFamilyMember(int id, [FromBody] CreateEmployeeFamilyMemberDto dto) =>
        Ok(await _service.AddFamilyMemberAsync(OrgId, id, dto));

    [Authorize(Policy = "employees.edit")]
    [HttpDelete("{id}/family-members/{memberId}")]
    public async Task<IActionResult> RemoveFamilyMember(int id, int memberId)
    {
        await _service.RemoveFamilyMemberAsync(OrgId, id, memberId);
        return NoContent();
    }

    [Authorize(Policy = "employees.edit")]
    [HttpPost("{id}/lifecycle-events")]
    public async Task<IActionResult> AddLifecycleEvent(int id, [FromBody] CreateLifecycleEventDto dto) =>
        Ok(await _service.AddLifecycleEventAsync(OrgId, id, dto));
}
