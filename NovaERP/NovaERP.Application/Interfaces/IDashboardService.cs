using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IDashboardService
{
    Task<List<DashboardDto>> GetAllAsync(int orgId, int userId);
    Task<DashboardDto> GetByIdAsync(int orgId, int userId, int id);
    Task<DashboardDto> CreateAsync(int orgId, int userId, CreateDashboardDto dto);
    Task<DashboardDto> UpdateAsync(int orgId, int userId, int id, UpdateDashboardDto dto);
    Task DeleteAsync(int orgId, int userId, int id);
    Task<DashboardDto> AddWidgetAsync(int orgId, int userId, int dashboardId, AddWidgetDto dto);
    Task<DashboardDto> RemoveWidgetAsync(int orgId, int userId, int dashboardId, int widgetId);
    Task<DashboardDto> ReorderWidgetsAsync(int orgId, int userId, int dashboardId, ReorderWidgetsDto dto);
}
