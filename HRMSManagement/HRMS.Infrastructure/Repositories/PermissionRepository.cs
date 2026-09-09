using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly HrmsDbContext _ctx;

    public PermissionRepository(HrmsDbContext ctx) => _ctx = ctx;

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
