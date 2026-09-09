using FoodOrder.Application.DTOs.Settings;

namespace FoodOrder.Application.Interfaces.Services;

public interface ISettingsService
{
    Task<SettingsDto> GetSettingsAsync();
    Task<SettingsDto> UpdateSettingsAsync(UpdateSettingsDto dto);
    Task<SettingsDto> GetOrgSettingsAsync(int orgId);
    Task<SettingsDto> UpsertOrgSettingsAsync(int orgId, UpdateSettingsDto dto);
}
