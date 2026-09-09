using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly PharmacyDbContext _ctx;

    public PermissionRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Permission>> GetAllAsync() =>
        _ctx.Permissions.OrderBy(p => p.Module).ThenBy(p => p.Key).ToListAsync();
}
