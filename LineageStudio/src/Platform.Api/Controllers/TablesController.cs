using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.DataDesigner;
using Platform.Runtime.Execution;

namespace Platform.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/applications/{applicationId:guid}/tables")]
public class TablesController : ControllerBase
{
    private readonly IDataTableService _tables;
    private readonly IRuntimeExecutionService _runtime;

    public TablesController(IDataTableService tables, IRuntimeExecutionService runtime)
    {
        _tables = tables;
        _runtime = runtime;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TableDto>>> List(Guid applicationId, CancellationToken ct)
        => Ok(await _tables.ListAsync(applicationId, ct));

    [HttpGet("{tableId:guid}")]
    public async Task<ActionResult<TableDto>> Get(Guid applicationId, Guid tableId, CancellationToken ct)
        => Ok(await _tables.GetAsync(applicationId, tableId, ct));

    /// <summary>The most-recently-written real rows of this table - "what data has actually
    /// flowed into the database", for the Data Designer's latest-data viewer.</summary>
    [HttpGet("{tableId:guid}/rows")]
    public async Task<ActionResult<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ListLatestRows(
        Guid applicationId, Guid tableId, [FromQuery] int limit, CancellationToken ct)
        => Ok(await _runtime.ListLatestRowsAsync(applicationId, tableId, limit <= 0 ? 10 : limit, ct));

    [HttpPost]
    public async Task<ActionResult<TableDto>> Create(Guid applicationId, CreateTableRequest request, CancellationToken ct)
    {
        var created = await _tables.CreateAsync(applicationId, request, ct);
        return CreatedAtAction(nameof(Get), new { applicationId, tableId = created.Id }, created);
    }

    [HttpDelete("{tableId:guid}")]
    public async Task<IActionResult> Delete(Guid applicationId, Guid tableId, CancellationToken ct)
    {
        await _tables.DeleteAsync(applicationId, tableId, ct);
        return NoContent();
    }

    [HttpPost("{tableId:guid}/columns")]
    public async Task<ActionResult<ColumnDto>> AddColumn(Guid applicationId, Guid tableId, AddColumnRequest request, CancellationToken ct)
    {
        var created = await _tables.AddColumnAsync(applicationId, tableId, request, ct);
        return CreatedAtAction(nameof(Get), new { applicationId, tableId }, created);
    }

    [HttpDelete("{tableId:guid}/columns/{columnId:guid}")]
    public async Task<IActionResult> DeleteColumn(Guid applicationId, Guid tableId, Guid columnId, CancellationToken ct)
    {
        await _tables.DeleteColumnAsync(applicationId, tableId, columnId, ct);
        return NoContent();
    }
}
