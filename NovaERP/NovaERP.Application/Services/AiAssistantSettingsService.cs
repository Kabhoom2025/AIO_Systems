using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class AiAssistantSettingsService : IAiAssistantSettingsService
{
    private readonly IAiAssistantSettingsRepository _repo;

    public AiAssistantSettingsService(IAiAssistantSettingsRepository repo) => _repo = repo;

    public async Task<AiAssistantSettingsDto> GetAsync(int orgId)
    {
        var settings = await _repo.GetByOrgAsync(orgId);
        // No row yet is the normal default state for a freshly-created org.
        if (settings == null)
            return new AiAssistantSettingsDto
            {
                OrganizationId = orgId,
                Provider = "Anthropic",
                AnthropicModel = "claude-sonnet-4-5",
                GroqModel = "llama-3.3-70b-versatile",
                IsConfigured = false
            };

        return ToDto(settings);
    }

    public async Task<AiAssistantSettingsDto> UpdateAsync(int orgId, UpdateAiAssistantSettingsDto dto)
    {
        var settings = await _repo.GetByOrgAsync(orgId);
        if (settings == null)
        {
            settings = new AiAssistantSettings { OrganizationId = orgId };
            ApplyUpdate(settings, dto);
            _repo.Add(settings);
        }
        else
        {
            ApplyUpdate(settings, dto);
            settings.UpdatedDate = DateTime.UtcNow;
            _repo.Update(settings);
        }

        await _repo.SaveChangesAsync();
        return ToDto(settings);
    }

    private static void ApplyUpdate(AiAssistantSettings settings, UpdateAiAssistantSettingsDto dto)
    {
        settings.Provider = string.IsNullOrWhiteSpace(dto.Provider) ? "Anthropic" : dto.Provider;

        if (!string.IsNullOrWhiteSpace(dto.AnthropicApiKey))
            settings.AnthropicApiKey = dto.AnthropicApiKey;
        settings.AnthropicModel = string.IsNullOrWhiteSpace(dto.AnthropicModel) ? "claude-sonnet-4-5" : dto.AnthropicModel;

        if (!string.IsNullOrWhiteSpace(dto.GroqApiKey))
            settings.GroqApiKey = dto.GroqApiKey;
        settings.GroqModel = string.IsNullOrWhiteSpace(dto.GroqModel) ? "llama-3.3-70b-versatile" : dto.GroqModel;
    }

    private static AiAssistantSettingsDto ToDto(AiAssistantSettings settings) => new()
    {
        OrganizationId = settings.OrganizationId,
        Provider = settings.Provider,
        AnthropicModel = settings.AnthropicModel,
        GroqModel = settings.GroqModel,
        IsConfigured = settings.Provider == "Groq"
            ? !string.IsNullOrWhiteSpace(settings.GroqApiKey)
            : !string.IsNullOrWhiteSpace(settings.AnthropicApiKey)
    };
}
