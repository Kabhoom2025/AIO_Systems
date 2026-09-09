using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface IPlatformModuleRepository
{
    Task<List<PlatformModule>>    GetAllAsync();
    Task<PlatformModule?>         GetByIdAsync(int id);
    Task<bool>                    KeyExistsAsync(string key);
    Task<List<string>>            GetEnabledKeysForOrgAsync(int orgId);
    Task<List<OrganizationModule>> GetOrgModulesAsync(int orgId);
    void                          Add(PlatformModule module);
    void                          Update(PlatformModule module);
    void                          Remove(PlatformModule module);
    void                          AddOrgModule(OrganizationModule om);
    void                          RemoveOrgModules(List<OrganizationModule> oms);
    Task                          SaveChangesAsync();
}
