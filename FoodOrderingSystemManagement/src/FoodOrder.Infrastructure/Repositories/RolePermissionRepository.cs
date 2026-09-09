using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly AppDbContext _context;

    public RolePermissionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetByRoleIdAsync(int roleId)
    {
        return await _context.RolePermissions
            .Where(p => p.RoleId == roleId)
            .Select(p => p.Feature)
            .OrderBy(f => f)
            .ToListAsync();
    }

    public async Task SaveForRoleAsync(int roleId, IEnumerable<string> features)
    {
        var existing = await _context.RolePermissions
            .Where(p => p.RoleId == roleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(existing);

        var newPerms = features
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Distinct()
            .Select(f => new RolePermission { RoleId = roleId, Feature = f.Trim() })
            .ToList();

        if (newPerms.Count > 0)
            await _context.RolePermissions.AddRangeAsync(newPerms);

        await _context.SaveChangesAsync();
    }
}
