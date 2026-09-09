using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IAiAssistantSettingsService
{
    Task<AiAssistantSettingsDto> GetAsync(int orgId);
    Task<AiAssistantSettingsDto> UpdateAsync(int orgId, UpdateAiAssistantSettingsDto dto);
}
