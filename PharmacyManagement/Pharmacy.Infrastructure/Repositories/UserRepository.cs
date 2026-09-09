using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PharmacyDbContext _ctx;

    public UserRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<User>> GetAllByOrgAsync(int orgId) =>
        _ctx.Users
            .Include(u => u.Role)
            .Where(u => u.OrganizationId == orgId && u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();
}
