using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IAiAssistantSettingsRepository
{
    Task<AiAssistantSettings?> GetByOrgAsync(int orgId);
    void Add(AiAssistantSettings settings);
    void Update(AiAssistantSettings settings);
    Task SaveChangesAsync();
}
