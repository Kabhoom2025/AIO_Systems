using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/recruitment")]
[Authorize]
public class RecruitmentController : ApiControllerBase
{
    private readonly IRecruitmentService _service;

    public RecruitmentController(IRecruitmentService service) => _service = service;

    // Job openings
    [Authorize(Policy = "recruitment.view")]
    [HttpGet("openings")]
    public async Task<IActionResult> GetOpenings([FromQuery] string? status) =>
        Ok(await _service.GetOpeningsAsync(OrgId, status));

    [Authorize(Policy = "recruitment.view")]
    [HttpGet("openings/{id}")]
    public async Task<IActionResult> GetOpening(int id) =>
        Ok(await _service.GetOpeningAsync(OrgId, id));

    [Authorize(Policy = "recruitment.create")]
    [HttpPost("openings")]
    public async Task<IActionResult> CreateOpening([FromBody] CreateJobOpeningDto dto)
    {
        var result = await _service.CreateOpeningAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetOpening), new { id = result.Id }, result);
    }

    [Authorize(Policy = "recruitment.edit")]
    [HttpPut("openings/{id}")]
    public async Task<IActionResult> UpdateOpening(int id, [FromBody] UpdateJobOpeningDto dto) =>
        Ok(await _service.UpdateOpeningAsync(OrgId, id, dto));

    [Authorize(Policy = "recruitment.delete")]
    [HttpDelete("openings/{id}")]
    public async Task<IActionResult> DeleteOpening(int id)
    {
        await _service.DeleteOpeningAsync(OrgId, id);
        return NoContent();
    }

    // Candidates
    [Authorize(Policy = "recruitment.view")]
    [HttpGet("openings/{openingId}/candidates")]
    public async Task<IActionResult> GetCandidates(int openingId) =>
        Ok(await _service.GetCandidatesAsync(OrgId, openingId));

    [Authorize(Policy = "recruitment.view")]
    [HttpGet("candidates/{id}")]
    public async Task<IActionResult> GetCandidate(int id) =>
        Ok(await _service.GetCandidateAsync(OrgId, id));

    [Authorize(Policy = "recruitment.create")]
    [HttpPost("openings/{openingId}/candidates")]
    public async Task<IActionResult> CreateCandidate(int openingId, [FromBody] CreateCandidateDto dto)
    {
        var result = await _service.CreateCandidateAsync(OrgId, openingId, dto);
        return CreatedAtAction(nameof(GetCandidate), new { id = result.Id }, result);
    }

    [Authorize(Policy = "recruitment.edit")]
    [HttpPut("candidates/{id}")]
    public async Task<IActionResult> UpdateCandidate(int id, [FromBody] UpdateCandidateDto dto) =>
        Ok(await _service.UpdateCandidateAsync(OrgId, id, dto));

    [Authorize(Policy = "recruitment.edit")]
    [HttpPost("candidates/{id}/stage")]
    public async Task<IActionResult> ChangeStage(int id, [FromBody] ChangeCandidateStageDto dto) =>
        Ok(await _service.ChangeStageAsync(OrgId, id, dto, UserName));

    [Authorize(Policy = "recruitment.delete")]
    [HttpDelete("candidates/{id}")]
    public async Task<IActionResult> DeleteCandidate(int id)
    {
        await _service.DeleteCandidateAsync(OrgId, id);
        return NoContent();
    }

    // Interviews
    [Authorize(Policy = "recruitment.view")]
    [HttpGet("interviews")]
    public async Task<IActionResult> GetUpcomingInterviews() =>
        Ok(await _service.GetUpcomingInterviewsAsync(OrgId));

    [Authorize(Policy = "recruitment.create")]
    [HttpPost("candidates/{candidateId}/interviews")]
    public async Task<IActionResult> ScheduleInterview(int candidateId, [FromBody] CreateInterviewDto dto) =>
        Ok(await _service.ScheduleInterviewAsync(OrgId, candidateId, dto));

    [Authorize(Policy = "recruitment.edit")]
    [HttpPut("interviews/{id}")]
    public async Task<IActionResult> UpdateInterview(int id, [FromBody] UpdateInterviewDto dto) =>
        Ok(await _service.UpdateInterviewAsync(OrgId, id, dto));

    [Authorize(Policy = "recruitment.edit")]
    [HttpPost("interviews/{id}/feedback")]
    public async Task<IActionResult> SubmitInterviewFeedback(int id, [FromBody] InterviewFeedbackDto dto) =>
        Ok(await _service.SubmitInterviewFeedbackAsync(OrgId, id, dto));

    // Pipeline
    [Authorize(Policy = "recruitment.view")]
    [HttpGet("pipeline")]
    public async Task<IActionResult> GetPipeline() =>
        Ok(await _service.GetPipelineAsync(OrgId));
}
