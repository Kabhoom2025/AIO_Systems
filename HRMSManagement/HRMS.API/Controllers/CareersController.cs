using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

/// <summary>Fully public — no authentication. Powers the "Apply now" link HR pastes into LinkedIn/Naukri job posts,
/// and the candidate's status-tracking link. No [Authorize] anywhere in this controller by design.</summary>
[ApiController]
[Route("api/careers")]
public class CareersController : ControllerBase
{
    private readonly ICareersService _service;

    public CareersController(ICareersService service) => _service = service;

    [HttpGet("org/{orgCode}")]
    public async Task<IActionResult> GetOrg(string orgCode) =>
        Ok(await _service.GetOrgAsync(orgCode));

    [HttpGet("org/{orgCode}/openings")]
    public async Task<IActionResult> GetOpenings(string orgCode) =>
        Ok(await _service.GetOpeningsAsync(orgCode));

    [HttpGet("org/{orgCode}/openings/{id}")]
    public async Task<IActionResult> GetOpening(string orgCode, int id) =>
        Ok(await _service.GetOpeningAsync(orgCode, id));

    [HttpPost("org/{orgCode}/openings/{id}/apply")]
    public async Task<IActionResult> Apply(string orgCode, int id, [FromBody] ApplyToJobDto dto) =>
        Ok(await _service.ApplyAsync(orgCode, id, dto));

    [HttpGet("track/{token}")]
    public async Task<IActionResult> Track(string token) =>
        Ok(await _service.TrackAsync(token));
}
