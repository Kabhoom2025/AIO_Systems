using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.ApiDesigner;

namespace Platform.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/applications/{applicationId:guid}/apis")]
public class ApisController : ControllerBase
{
    private readonly IApiEndpointService _apis;

    public ApisController(IApiEndpointService apis)
    {
        _apis = apis;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApiEndpointDto>>> List(Guid applicationId, CancellationToken ct)
        => Ok(await _apis.ListAsync(applicationId, ct));

    [HttpGet("{apiId:guid}")]
    public async Task<ActionResult<ApiEndpointDto>> Get(Guid applicationId, Guid apiId, CancellationToken ct)
        => Ok(await _apis.GetAsync(applicationId, apiId, ct));

    [HttpPost]
    public async Task<ActionResult<ApiEndpointDto>> Create(Guid applicationId, CreateApiEndpointRequest request, CancellationToken ct)
    {
        var created = await _apis.CreateAsync(applicationId, request, ct);
        return CreatedAtAction(nameof(Get), new { applicationId, apiId = created.Id }, created);
    }

    [HttpPut("{apiId:guid}")]
    public async Task<ActionResult<ApiEndpointDto>> Update(Guid applicationId, Guid apiId, UpdateApiEndpointRequest request, CancellationToken ct)
        => Ok(await _apis.UpdateAsync(applicationId, apiId, request, ct));

    [HttpDelete("{apiId:guid}")]
    public async Task<IActionResult> Delete(Guid applicationId, Guid apiId, CancellationToken ct)
    {
        await _apis.DeleteAsync(applicationId, apiId, ct);
        return NoContent();
    }
}
