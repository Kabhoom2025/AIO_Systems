using FoodOrder.Application.DTOs.PlatformModule;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class PlatformModuleService : IPlatformModuleService
{
    private readonly IPlatformModuleRepository _repo;
    private readonly IOrganizationRepository   _orgRepo;

    public PlatformModuleService(IPlatformModuleRepository repo, IOrganizationRepository orgRepo)
    {
        _repo    = repo;
        _orgRepo = orgRepo;
    }

    public async Task<List<PlatformModuleDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<PlatformModuleDto> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new AppException("Platform module not found.", 404);
        return ToDto(entity);
    }

    public async Task<PlatformModuleDto> CreateAsync(CreatePlatformModuleDto dto)
    {
        var key = dto.Key.ToLower().Replace(' ', '_');
        if (await _repo.KeyExistsAsync(key))
            throw new AppException($"A module with key '{key}' already exists.", 400);

        var entity = new PlatformModule
        {
            Name        = dto.Name,
            Key         = key,
            Description = dto.Description,
            Icon        = dto.Icon,
            Color       = dto.Color,
            SortOrder   = dto.SortOrder,
            IsActive    = true,
        };
        _repo.Add(entity);
        await _repo.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<PlatformModuleDto> UpdateAsync(int id, UpdatePlatformModuleDto dto)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new AppException("Platform module not found.", 404);

        entity.Name        = dto.Name;
        entity.Description = dto.Description;
        entity.Icon        = dto.Icon;
        entity.Color       = dto.Color;
        entity.IsActive    = dto.IsActive;
        entity.SortOrder   = dto.SortOrder;

        _repo.Update(entity);
        await _repo.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new AppException("Platform module not found.", 404);
        _repo.Remove(entity);
        await _repo.SaveChangesAsync();
    }

    public Task<List<string>> GetEnabledModuleKeysForOrgAsync(int orgId) =>
        _repo.GetEnabledKeysForOrgAsync(orgId);

    public async Task<List<OrgModuleStatusDto>> GetAllOrgModuleStatusAsync()
    {
        var orgs       = (await _orgRepo.GetAllAsync()).OrderBy(o => o.Name).ToList();
        var allModules = await _repo.GetAllAsync();
        var result     = new List<OrgModuleStatusDto>();

        foreach (var org in orgs)
        {
            var enabledKeys = await _repo.GetEnabledKeysForOrgAsync(org.Id);
            var enabledSet  = enabledKeys.ToHashSet();

            result.Add(new OrgModuleStatusDto
            {
                OrganizationId   = org.Id,
                OrganizationName = org.Name,
                Modules = allModules.Select(m => new OrgModuleItemDto
                {
                    ModuleId   = m.Id,
                    ModuleName = m.Name,
                    ModuleKey  = m.Key,
                    Icon       = m.Icon,
                    Color      = m.Color,
                    IsEnabled  = enabledSet.Contains(m.Key),
                }).ToList(),
            });
        }
        return result;
    }

    public async Task<OrgModuleStatusDto> GetOrgModuleStatusAsync(int orgId)
    {
        var org = await _orgRepo.GetByIdAsync(orgId)
            ?? throw new AppException("Organization not found.", 404);

        var allModules  = await _repo.GetAllAsync();
        var enabledKeys = await _repo.GetEnabledKeysForOrgAsync(orgId);
        var enabledSet  = enabledKeys.ToHashSet();

        return new OrgModuleStatusDto
        {
            OrganizationId   = org.Id,
            OrganizationName = org.Name,
            Modules = allModules.Select(m => new OrgModuleItemDto
            {
                ModuleId   = m.Id,
                ModuleName = m.Name,
                ModuleKey  = m.Key,
                Icon       = m.Icon,
                Color      = m.Color,
                IsEnabled  = enabledSet.Contains(m.Key),
            }).ToList(),
        };
    }

    public async Task AssignModulesToOrgAsync(OrgModuleAssignmentDto dto)
    {
        var existing = await _repo.GetOrgModulesAsync(dto.OrganizationId);
        _repo.RemoveOrgModules(existing);

        foreach (var moduleId in dto.ModuleIds)
        {
            _repo.AddOrgModule(new OrganizationModule
            {
                OrganizationId   = dto.OrganizationId,
                PlatformModuleId = moduleId,
                IsEnabled        = true,
            });
        }
        await _repo.SaveChangesAsync();
    }

    private static PlatformModuleDto ToDto(PlatformModule p) => new()
    {
        Id          = p.Id,
        Name        = p.Name,
        Key         = p.Key,
        Description = p.Description,
        Icon        = p.Icon,
        Color       = p.Color,
        IsActive    = p.IsActive,
        SortOrder   = p.SortOrder,
    };
}
