using AIO_Systems.Data;
using AIO_Systems.Domain.Entities;
using AIO_Systems.DTOs.PlatformModule;
using AIO_Systems.Services.Interfaces;
using AIO_Systems.Shared;
using Microsoft.EntityFrameworkCore;

namespace AIO_Systems.Services;

public class PlatformModuleService(AppDbContext db) : IPlatformModuleService
{
    public async Task<List<PlatformModuleDto>> GetAllAsync()
    {
        var list = await db.PlatformModules.ToListAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<PlatformModuleDto> GetByIdAsync(int id)
    {
        var entity = await db.PlatformModules.FindAsync(id)
            ?? throw new AppException("Platform module not found.", 404);
        return ToDto(entity);
    }

    public async Task<PlatformModuleDto> CreateAsync(CreatePlatformModuleDto dto)
    {
        var key = dto.Key.ToLower().Replace(' ', '_');
        if (await db.PlatformModules.AnyAsync(m => m.Key == key))
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
        db.PlatformModules.Add(entity);
        await db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<PlatformModuleDto> UpdateAsync(int id, UpdatePlatformModuleDto dto)
    {
        var entity = await db.PlatformModules.FindAsync(id)
            ?? throw new AppException("Platform module not found.", 404);

        entity.Name        = dto.Name;
        entity.Description = dto.Description;
        entity.Icon        = dto.Icon;
        entity.Color       = dto.Color;
        entity.IsActive    = dto.IsActive;
        entity.SortOrder   = dto.SortOrder;

        await db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await db.PlatformModules.FindAsync(id)
            ?? throw new AppException("Platform module not found.", 404);
        db.PlatformModules.Remove(entity);
        await db.SaveChangesAsync();
    }

    public Task<List<string>> GetEnabledModuleKeysForOrgAsync(int orgId) =>
        db.OrganizationModules
            .Where(om => om.OrganizationId == orgId && om.IsEnabled)
            .Select(om => om.PlatformModule.Key)
            .ToListAsync();

    public async Task<List<OrgModuleStatusDto>> GetAllOrgModuleStatusAsync()
    {
        var orgs       = await db.Organizations.OrderBy(o => o.Name).ToListAsync();
        var allModules = await db.PlatformModules.ToListAsync();
        var result     = new List<OrgModuleStatusDto>();

        foreach (var org in orgs)
        {
            var enabledSet = (await GetEnabledModuleKeysForOrgAsync(org.Id)).ToHashSet();

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
        var org = await db.Organizations.FindAsync(orgId)
            ?? throw new AppException("Organization not found.", 404);

        var allModules = await db.PlatformModules.ToListAsync();
        var enabledSet = (await GetEnabledModuleKeysForOrgAsync(orgId)).ToHashSet();

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
        var existing = await db.OrganizationModules
            .Where(om => om.OrganizationId == dto.OrganizationId)
            .ToListAsync();
        db.OrganizationModules.RemoveRange(existing);

        foreach (var moduleId in dto.ModuleIds)
        {
            db.OrganizationModules.Add(new OrganizationModule
            {
                OrganizationId   = dto.OrganizationId,
                PlatformModuleId = moduleId,
                IsEnabled        = true,
            });
        }
        await db.SaveChangesAsync();
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
