using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.MappingDesigner;

namespace Platform.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/applications/{applicationId:guid}/mappings")]
public class MappingsController : ControllerBase
{
    private readonly IMappingService _mappings;

    public MappingsController(IMappingService mappings)
    {
        _mappings = mappings;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MappingDto>>> List(Guid applicationId, CancellationToken ct)
        => Ok(await _mappings.ListAsync(applicationId, ct));

    [HttpGet("{mappingId:guid}")]
    public async Task<ActionResult<MappingDto>> Get(Guid applicationId, Guid mappingId, CancellationToken ct)
        => Ok(await _mappings.GetAsync(applicationId, mappingId, ct));

    [HttpPost]
    public async Task<ActionResult<MappingDto>> Create(Guid applicationId, CreateMappingRequest request, CancellationToken ct)
    {
        var created = await _mappings.CreateAsync(applicationId, request, ct);
        return CreatedAtAction(nameof(Get), new { applicationId, mappingId = created.Id }, created);
    }

    [HttpPut("{mappingId:guid}")]
    public async Task<ActionResult<MappingDto>> Update(Guid applicationId, Guid mappingId, UpdateMappingRequest request, CancellationToken ct)
        => Ok(await _mappings.UpdateAsync(applicationId, mappingId, request, ct));

    [HttpDelete("{mappingId:guid}")]
    public async Task<IActionResult> Delete(Guid applicationId, Guid mappingId, CancellationToken ct)
    {
        await _mappings.DeleteAsync(applicationId, mappingId, ct);
        return NoContent();
    }
}
