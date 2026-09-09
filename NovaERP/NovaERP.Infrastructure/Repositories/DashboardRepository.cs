using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly NovaErpDbContext _ctx;

    public DashboardRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<Dashboard> Query() =>
        _ctx.Dashboards.Include(d => d.Widgets);

    public Task<List<Dashboard>> GetAllByUserAsync(int orgId, int userId) =>
        Query().Where(d => d.OrganizationId == orgId && d.UserId == userId)
            .OrderBy(d => d.Name)
            .ToListAsync();

    public Task<Dashboard?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(d => d.Id == id && d.OrganizationId == orgId);

    public void Add(Dashboard dashboard)    => _ctx.Dashboards.Add(dashboard);
    public void Update(Dashboard dashboard) => _ctx.Dashboards.Update(dashboard);
    public void Remove(Dashboard dashboard) => _ctx.Dashboards.Remove(dashboard);

    public void AddWidget(DashboardWidget widget)    => _ctx.Set<DashboardWidget>().Add(widget);
    public void RemoveWidget(DashboardWidget widget) => _ctx.Set<DashboardWidget>().Remove(widget);

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
