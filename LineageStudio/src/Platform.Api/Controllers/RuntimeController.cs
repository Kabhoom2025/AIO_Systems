using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Runtime.Execution;

namespace Platform.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/runtime/{applicationId:guid}")]
public class RuntimeController : ControllerBase
{
    private readonly IRuntimeExecutionService _runtime;

    public RuntimeController(IRuntimeExecutionService runtime)
    {
        _runtime = runtime;
    }

    [HttpPost("execute")]
    public async Task<ActionResult<RuntimeExecutionResult>> Execute(Guid applicationId, RuntimeExecuteRequest request, CancellationToken ct)
        => Ok(await _runtime.ExecuteAsync(applicationId, request, ct));
}
