using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

/// <summary>
/// Minimal settings access needed by the Order module.
/// Expanded with update support in Step 7 (Settings module).
/// </summary>
public interface ISettingsRepository : IGenericRepository<Settings>
{
    Task<Settings?> GetSettingsAsync();
    Task<Settings?> GetByOrgAsync(int orgId);
}
