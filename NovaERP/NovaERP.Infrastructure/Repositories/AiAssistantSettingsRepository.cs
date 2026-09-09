using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class AiAssistantSettingsRepository : IAiAssistantSettingsRepository
{
    private readonly NovaErpDbContext _ctx;

    public AiAssistantSettingsRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<AiAssistantSettings?> GetByOrgAsync(int orgId) =>
        _ctx.AiAssistantSettings.FirstOrDefaultAsync(s => s.OrganizationId == orgId);

    public void Add(AiAssistantSettings settings) => _ctx.AiAssistantSettings.Add(settings);
    public void Update(AiAssistantSettings settings) => _ctx.AiAssistantSettings.Update(settings);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
