using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class NotificationChannelSettingsRepository : INotificationChannelSettingsRepository
{
    private readonly NovaErpDbContext _ctx;

    public NotificationChannelSettingsRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<NotificationChannelSettings?> GetByOrgAsync(int orgId) =>
        _ctx.NotificationChannelSettings.FirstOrDefaultAsync(s => s.OrganizationId == orgId);

    public void Add(NotificationChannelSettings settings) => _ctx.NotificationChannelSettings.Add(settings);
    public void Update(NotificationChannelSettings settings) => _ctx.NotificationChannelSettings.Update(settings);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
