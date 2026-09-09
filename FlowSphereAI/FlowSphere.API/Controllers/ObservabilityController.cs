using FlowSphere.Application.Observability.Queries.GetExecutionMetrics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[Authorize]
[Route("api/observability")]
public class ObservabilityController : ApiControllerBase
{
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetExecutionMetricsQuery(), cancellationToken);
        return FromResult(result);
    }
}
