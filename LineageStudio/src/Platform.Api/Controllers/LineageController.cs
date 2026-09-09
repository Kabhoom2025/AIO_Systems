using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Lineage.Query;
using Platform.Lineage.Recording;

namespace Platform.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/lineage")]
public class LineageController : ControllerBase
{
    private readonly ILineageQueryService _lineage;

    public LineageController(ILineageQueryService lineage)
    {
        _lineage = lineage;
    }

    [HttpGet("executions")]
    public async Task<ActionResult<IReadOnlyList<LineageExecutionDto>>> ListExecutions([FromQuery] Guid? applicationId, CancellationToken ct)
        => Ok(await _lineage.ListExecutionsAsync(applicationId, ct));

    [HttpGet("executions/{executionId:guid}")]
    public async Task<ActionResult<LineageExecutionDto>> GetExecution(Guid executionId, CancellationToken ct)
        => Ok(await _lineage.GetExecutionAsync(executionId, ct));

    [HttpGet("{executionId:guid}/events")]
    public async Task<ActionResult<IReadOnlyList<LineageEventDto>>> ListEvents(Guid executionId, CancellationToken ct)
        => Ok(await _lineage.ListEventsAsync(executionId, ct));
}
