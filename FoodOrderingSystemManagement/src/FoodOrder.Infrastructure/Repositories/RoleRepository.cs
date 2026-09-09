using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Role>> GetAllWithUsersAsync(int? organizationId = null)
    {
        return await _context.Roles
            .Where(r => r.RoleName != "SuperAdmin")  // never expose SuperAdmin role at org level
            .Include(r => r.Users.Where(u =>
                organizationId.HasValue
                    ? u.OrganizationId == organizationId.Value
                    : u.OrganizationId != null))
            .OrderBy(r => r.Id)
            .ToListAsync();
    }

    public async Task<Role?> GetByNameAsync(string roleName) =>
        await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName.ToLower() == roleName.ToLower().Trim());
}
