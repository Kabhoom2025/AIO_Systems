using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Infrastructure.Jobs;

namespace NovaERP.API.Controllers;

[Route("api/scheduled-jobs")]
[Authorize]
public class ScheduledJobController : ApiControllerBase
{
    private readonly IScheduledJobService _service;

    public ScheduledJobController(IScheduledJobService service) => _service = service;

    [Authorize(Policy = "scheduler.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    // .edit is reused for both mutating actions on this controller (update + trigger-now),
    // matching the workflow-engine agent's precedent of reusing .edit for approve/reject.
    [Authorize(Policy = "scheduler.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateScheduledJobDto dto)
    {
        var result = await _service.UpdateAsync(OrgId, id, dto);
        RecurringJobRegistrar.Register(result.JobKey, result.Id, result.CronExpression, result.IsEnabled);
        return Ok(result);
    }

    [Authorize(Policy = "scheduler.edit")]
    [HttpPost("{id}/trigger-now")]
    public async Task<IActionResult> TriggerNow(int id)
    {
        var job = await _service.GetByIdAsync(OrgId, id);
        var backgroundJobId = RecurringJobRegistrar.TriggerNow(job.JobKey);
        return Ok(new { queuedJobId = backgroundJobId });
    }
}
