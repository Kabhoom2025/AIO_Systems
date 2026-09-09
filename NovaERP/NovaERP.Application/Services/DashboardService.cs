using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

/// <summary>A Dashboard belongs to a specific User, not a Role — every authenticated user
/// manages their own, the same self-service shape as Employee/Customer/Vendor Portal. Unlike
/// those modules there's no "unlinked" case (every User can own dashboards), so the guard here
/// is "exists but not mine" -&gt; UnauthorizedAccessException -&gt; 403, not a 404, mirroring
/// EmployeePortalService.CancelMyLeaveRequestAsync's ownership check.</summary>
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repo;
    private readonly IValidator<CreateDashboardDto> _createValidator;
    private readonly IValidator<UpdateDashboardDto> _updateValidator;
    private readonly IValidator<AddWidgetDto> _addWidgetValidator;
    private readonly IValidator<ReorderWidgetsDto> _reorderValidator;

    public DashboardService(IDashboardRepository repo,
        IValidator<CreateDashboardDto> createValidator, IValidator<UpdateDashboardDto> updateValidator,
        IValidator<AddWidgetDto> addWidgetValidator, IValidator<ReorderWidgetsDto> reorderValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _addWidgetValidator = addWidgetValidator;
        _reorderValidator = reorderValidator;
    }

    public async Task<List<DashboardDto>> GetAllAsync(int orgId, int userId)
    {
        var dashboards = await _repo.GetAllByUserAsync(orgId, userId);
        return dashboards.Select(ToDto).ToList();
    }

    public async Task<DashboardDto> GetByIdAsync(int orgId, int userId, int id)
    {
        var dashboard = await GetOwnedAsync(orgId, userId, id);
        return ToDto(dashboard);
    }

    public async Task<DashboardDto> CreateAsync(int orgId, int userId, CreateDashboardDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var dashboard = new Dashboard
        {
            OrganizationId = orgId,
            UserId = userId,
            Name = dto.Name,
            IsDefault = dto.IsDefault
        };

        _repo.Add(dashboard);
        await _repo.SaveChangesAsync();
        return ToDto(dashboard);
    }

    public async Task<DashboardDto> UpdateAsync(int orgId, int userId, int id, UpdateDashboardDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var dashboard = await GetOwnedAsync(orgId, userId, id);

        dashboard.Name = dto.Name;
        dashboard.IsDefault = dto.IsDefault;
        dashboard.UpdatedDate = DateTime.UtcNow;

        _repo.Update(dashboard);
        await _repo.SaveChangesAsync();
        return ToDto(dashboard);
    }

    public async Task DeleteAsync(int orgId, int userId, int id)
    {
        var dashboard = await GetOwnedAsync(orgId, userId, id);
        _repo.Remove(dashboard);
        await _repo.SaveChangesAsync();
    }

    public async Task<DashboardDto> AddWidgetAsync(int orgId, int userId, int dashboardId, AddWidgetDto dto)
    {
        await _addWidgetValidator.ValidateAndThrowAsync(dto);

        var dashboard = await GetOwnedAsync(orgId, userId, dashboardId);

        var nextOrder = dashboard.Widgets.Any() ? dashboard.Widgets.Max(w => w.DisplayOrder) + 1 : 1;
        _repo.AddWidget(new DashboardWidget
        {
            DashboardId = dashboard.Id,
            WidgetType = dto.WidgetType,
            Title = dto.Title,
            SizeOption = dto.SizeOption,
            DisplayOrder = nextOrder
        });
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, dashboard.Id) ?? dashboard;
        return ToDto(reloaded);
    }

    public async Task<DashboardDto> RemoveWidgetAsync(int orgId, int userId, int dashboardId, int widgetId)
    {
        var dashboard = await GetOwnedAsync(orgId, userId, dashboardId);

        var widget = dashboard.Widgets.FirstOrDefault(w => w.Id == widgetId)
            ?? throw new KeyNotFoundException($"Widget {widgetId} not found");

        _repo.RemoveWidget(widget);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, dashboard.Id) ?? dashboard;
        return ToDto(reloaded);
    }

    public async Task<DashboardDto> ReorderWidgetsAsync(int orgId, int userId, int dashboardId, ReorderWidgetsDto dto)
    {
        await _reorderValidator.ValidateAndThrowAsync(dto);

        var dashboard = await GetOwnedAsync(orgId, userId, dashboardId);

        foreach (var entry in dto.Widgets)
        {
            var widget = dashboard.Widgets.FirstOrDefault(w => w.Id == entry.WidgetId);
            if (widget is null) continue;
            widget.DisplayOrder = entry.DisplayOrder;
            widget.SizeOption = entry.SizeOption;
        }

        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, dashboard.Id) ?? dashboard;
        return ToDto(reloaded);
    }

    private async Task<Dashboard> GetOwnedAsync(int orgId, int userId, int id)
    {
        var dashboard = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Dashboard {id} not found");

        if (dashboard.UserId != userId)
            throw new UnauthorizedAccessException("You can only access your own dashboards.");

        return dashboard;
    }

    private static DashboardDto ToDto(Dashboard d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        IsDefault = d.IsDefault,
        Widgets = d.Widgets.OrderBy(w => w.DisplayOrder).Select(w => new DashboardWidgetDto
        {
            Id = w.Id,
            WidgetType = w.WidgetType,
            Title = w.Title,
            SizeOption = w.SizeOption,
            DisplayOrder = w.DisplayOrder
        }).ToList()
    };
}
