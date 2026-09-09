using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Applications;
using Platform.Application.DataDesigner;
using Platform.Application.MappingDesigner;
using Platform.Application.UiBuilder;
using Platform.Runtime.Execution;

namespace Platform.Api.Controllers;

/// <summary>
/// The end-user-facing surface for a published application - no builder login required. Everything
/// else in this API (Applications/Screens/Apis/Mappings/Runtime controllers) is for the person
/// building the app; this controller is for someone actually using the finished, published app, so
/// every action here first confirms the application is published before touching it, regardless of
/// what its actual publish state does (or doesn't) mean elsewhere.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/public/apps/{applicationId:guid}")]
public class PublicController : ControllerBase
{
    private readonly IApplicationService _applications;
    private readonly IScreenService _screens;
    private readonly IComponentService _components;
    private readonly IMappingService _mappings;
    private readonly IDataTableService _tables;
    private readonly IRuntimeExecutionService _runtime;

    public PublicController(
        IApplicationService applications,
        IScreenService screens,
        IComponentService components,
        IMappingService mappings,
        IDataTableService tables,
        IRuntimeExecutionService runtime)
    {
        _applications = applications;
        _screens = screens;
        _components = components;
        _mappings = mappings;
        _tables = tables;
        _runtime = runtime;
    }

    [HttpGet]
    public async Task<ActionResult<ApplicationDto>> GetApp(Guid applicationId, CancellationToken ct)
        => Ok(await RequirePublishedAsync(applicationId, ct));

    [HttpGet("screens")]
    public async Task<ActionResult<IReadOnlyList<ScreenDto>>> ListScreens(Guid applicationId, CancellationToken ct)
    {
        await RequirePublishedAsync(applicationId, ct);
        return Ok(await _screens.ListAsync(applicationId, ct));
    }

    [HttpGet("screens/{screenId:guid}/components")]
    public async Task<ActionResult<IReadOnlyList<ComponentDto>>> ListComponents(Guid applicationId, Guid screenId, CancellationToken ct)
    {
        await RequirePublishedAsync(applicationId, ct);
        return Ok(await _components.ListAsync(applicationId, screenId, ct));
    }

    [HttpGet("mappings")]
    public async Task<ActionResult<IReadOnlyList<MappingDto>>> ListMappings(Guid applicationId, CancellationToken ct)
    {
        await RequirePublishedAsync(applicationId, ct);
        return Ok(await _mappings.ListAsync(applicationId, ct));
    }

    [HttpGet("tables")]
    public async Task<ActionResult<IReadOnlyList<TableDto>>> ListTables(Guid applicationId, CancellationToken ct)
    {
        await RequirePublishedAsync(applicationId, ct);
        return Ok(await _tables.ListAsync(applicationId, ct));
    }

    /// <summary>Existing rows of one of the app's own tables - used to populate a foreign-key
    /// picker (e.g. "which customer is this order for") with real, pickable rows instead of
    /// asking an end user to type a raw UUID.</summary>
    [HttpGet("tables/{tableId:guid}/rows")]
    public async Task<ActionResult<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ListTableRows(
        Guid applicationId, Guid tableId, CancellationToken ct)
    {
        await RequirePublishedAsync(applicationId, ct);
        return Ok(await _runtime.ListTableRowsAsync(applicationId, tableId, ct));
    }

    [HttpPost("execute")]
    public async Task<ActionResult<RuntimeExecutionResult>> Execute(Guid applicationId, RuntimeExecuteRequest request, CancellationToken ct)
    {
        await RequirePublishedAsync(applicationId, ct);
        return Ok(await _runtime.ExecuteAsync(applicationId, request, ct));
    }

    private async Task<ApplicationDto> RequirePublishedAsync(Guid applicationId, CancellationToken ct)
    {
        var application = await _applications.GetAsync(applicationId, ct);
        if (!application.IsPublished)
            throw new KeyNotFoundException($"Application '{applicationId}' is not published.");

        return application;
    }
}
