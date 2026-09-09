using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly NovaErpDbContext _ctx;

    public PermissionRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<Permission>> GetAllAsync() =>
        _ctx.Permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Key)
            .ToListAsync();

    public void Add(Permission permission)    => _ctx.Permissions.Add(permission);
    public void Update(Permission permission) => _ctx.Permissions.Update(permission);
    public void Remove(Permission permission) => _ctx.Permissions.Remove(permission);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
