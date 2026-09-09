using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _repo;

    public PermissionService(IPermissionRepository repo) => _repo = repo;

    public async Task<List<PermissionDto>> GetAllAsync()
    {
        var permissions = await _repo.GetAllAsync();
        return permissions.Select(MapToDto).ToList();
    }

    private static PermissionDto MapToDto(Permission p) => new()
    {
        Id          = p.Id,
        Key         = p.Key,
        Module      = p.Module,
        Description = p.Description
    };
}
