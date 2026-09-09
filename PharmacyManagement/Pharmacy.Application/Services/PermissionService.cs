using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

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
