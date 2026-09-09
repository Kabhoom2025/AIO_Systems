using FoodOrder.Application.DTOs.PlatformModule;

namespace FoodOrder.Application.Interfaces.Services;

public interface IPlatformModuleService
{
    Task<List<PlatformModuleDto>>   GetAllAsync();
    Task<PlatformModuleDto>         GetByIdAsync(int id);
    Task<PlatformModuleDto>         CreateAsync(CreatePlatformModuleDto dto);
    Task<PlatformModuleDto>         UpdateAsync(int id, UpdatePlatformModuleDto dto);
    Task                            DeleteAsync(int id);
    Task<List<string>>              GetEnabledModuleKeysForOrgAsync(int orgId);
    Task<List<OrgModuleStatusDto>>  GetAllOrgModuleStatusAsync();
    Task<OrgModuleStatusDto>        GetOrgModuleStatusAsync(int orgId);
    Task                            AssignModulesToOrgAsync(OrgModuleAssignmentDto dto);
}
