using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

/// <summary>Self-service dashboard builder — a Dashboard belongs to a User, not a Role, so
/// there is no permission policy on any action here (see IDashboardService's doc comment).
/// Widget-data is org-scoped shared operational data, not "my" data, so it needs no ownership
/// check.</summary>
[Route("api/my-dashboards")]
[Authorize]
public class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _service;
    private readonly IWidgetDataService _widgetDataService;

    public DashboardController(IDashboardService service, IWidgetDataService widgetDataService)
    {
        _service = service;
        _widgetDataService = widgetDataService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId, UserId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, UserId, id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDashboardDto dto)
    {
        var result = await _service.CreateAsync(OrgId, UserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDashboardDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, UserId, id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, UserId, id);
        return NoContent();
    }

    [HttpPost("{id}/widgets")]
    public async Task<IActionResult> AddWidget(int id, [FromBody] AddWidgetDto dto) =>
        Ok(await _service.AddWidgetAsync(OrgId, UserId, id, dto));

    [HttpDelete("{id}/widgets/{widgetId}")]
    public async Task<IActionResult> RemoveWidget(int id, int widgetId) =>
        Ok(await _service.RemoveWidgetAsync(OrgId, UserId, id, widgetId));

    [HttpPut("{id}/widgets/layout")]
    public async Task<IActionResult> ReorderWidgets(int id, [FromBody] ReorderWidgetsDto dto) =>
        Ok(await _service.ReorderWidgetsAsync(OrgId, UserId, id, dto));

    [HttpGet("widget-data/{widgetType}")]
    public async Task<IActionResult> GetWidgetData(string widgetType) =>
        Ok(await _widgetDataService.GetDataAsync(OrgId, widgetType));
}
