using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly PharmacyDbContext _ctx;

    public RoleRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Role>> GetAllAsync(int orgId) =>
        _ctx.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Where(r => r.OrganizationId == null || r.OrganizationId == orgId)
            .OrderBy(r => r.Name)
            .ToListAsync();

    public Task<Role?> GetByIdAsync(int id) =>
        _ctx.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id);

    public Task<List<Permission>> GetPermissionsByKeysAsync(List<string> keys) =>
        _ctx.Permissions.Where(p => keys.Contains(p.Key)).ToListAsync();

    public void Add(Role role)    => _ctx.Roles.Add(role);
    public void Remove(Role role) => _ctx.Roles.Remove(role);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
