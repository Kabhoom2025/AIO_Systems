using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class ScheduledJobRepository : IScheduledJobRepository
{
    private readonly NovaErpDbContext _ctx;

    public ScheduledJobRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<ScheduledJobDefinition>> GetVisibleToOrgAsync(int orgId) =>
        _ctx.ScheduledJobDefinitions
            .Where(j => j.OrganizationId == orgId || j.OrganizationId == null)
            .OrderBy(j => j.Name)
            .ToListAsync();

    public Task<ScheduledJobDefinition?> GetByIdAsync(int orgId, int id) =>
        _ctx.ScheduledJobDefinitions
            .FirstOrDefaultAsync(j => j.Id == id && (j.OrganizationId == orgId || j.OrganizationId == null));

    public Task<List<ScheduledJobDefinition>> GetAllEnabledAsync() =>
        _ctx.ScheduledJobDefinitions.Where(j => j.IsEnabled).ToListAsync();

    public void Update(ScheduledJobDefinition job) => _ctx.ScheduledJobDefinitions.Update(job);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
