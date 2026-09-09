using FlowSphere.Application.Tables.Commands.CreateTable;
using FlowSphere.Application.Tables.Commands.DeleteTable;
using FlowSphere.Application.Tables.Commands.PublishTable;
using FlowSphere.Application.Tables.Commands.SaveTableSchema;
using FlowSphere.Application.Tables.Queries.GetTableById;
using FlowSphere.Application.Tables.Queries.GetTableRecordsList;
using FlowSphere.Application.Tables.Queries.GetTablesList;
using FlowSphere.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[Authorize]
[Route("api")]
public class TablesController : ApiControllerBase
{
    [HttpGet("workspaces/{workspaceId:int}/tables")]
    [Authorize(Policy = PermissionCatalog.TablesRead)]
    public async Task<IActionResult> GetList(int workspaceId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetTablesListQuery(workspaceId), cancellationToken);
        return FromResult(result);
    }

    public record CreateTableRequest(string Name, string? Description);

    // tables.write is enforced by WorkspacePermissionBehavior (global-role grant OR a
    // workspace-scoped WorkspaceRoleAssignment both satisfy it) - the bare class-level
    // [Authorize] above still requires the caller to be authenticated and in this org.
    [HttpPost("workspaces/{workspaceId:int}/tables")]
    public async Task<IActionResult> Create(int workspaceId, CreateTableRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new CreateTableCommand(workspaceId, request.Name, request.Description), cancellationToken);
        return FromResult(result, id => CreatedAtAction(nameof(GetById), new { id }, new { id }));
    }

    [HttpGet("tables/{id:int}")]
    [Authorize(Policy = PermissionCatalog.TablesRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetTableByIdQuery(id), cancellationToken);
        return FromResult(result);
    }

    public record SaveSchemaRequest(string SchemaJson);

    [HttpPut("tables/{id:int}/schema")]
    public async Task<IActionResult> SaveSchema(int id, SaveSchemaRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SaveTableSchemaCommand(id, request.SchemaJson), cancellationToken);
        return FromResult(result);
    }

    [HttpPost("tables/{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new PublishTableCommand(id), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("tables/{id:int}/records")]
    [Authorize(Policy = PermissionCatalog.TablesRead)]
    public async Task<IActionResult> GetRecords(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetTableRecordsListQuery(id), cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("tables/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteTableCommand(id), cancellationToken);
        return FromResult(result);
    }
}
