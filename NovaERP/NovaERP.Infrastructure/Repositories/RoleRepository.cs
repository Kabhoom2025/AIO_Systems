using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly NovaErpDbContext _ctx;

    public RoleRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<Role>> GetAllForOrgAsync(int orgId) =>
        _ctx.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(r => r.Users)
            .Where(r => r.OrganizationId == null || r.OrganizationId == orgId)
            .OrderBy(r => r.Name)
            .ToListAsync();

    public Task<Role?> GetByIdAsync(int id) =>
        _ctx.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == id);

    public Task<List<Permission>> GetPermissionsByKeysAsync(List<string> keys) =>
        _ctx.Permissions.Where(p => keys.Contains(p.Key)).ToListAsync();

    public void Add(Role role)    => _ctx.Roles.Add(role);
    public void Update(Role role) => _ctx.Roles.Update(role);
    public void Remove(Role role) => _ctx.Roles.Remove(role);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
