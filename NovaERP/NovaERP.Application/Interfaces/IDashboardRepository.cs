using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IDashboardRepository
{
    Task<List<Dashboard>> GetAllByUserAsync(int orgId, int userId);
    Task<Dashboard?> GetByIdAsync(int orgId, int id);

    void Add(Dashboard dashboard);
    void Update(Dashboard dashboard);
    void Remove(Dashboard dashboard);
    void AddWidget(DashboardWidget widget);
    void RemoveWidget(DashboardWidget widget);
    Task SaveChangesAsync();
}
