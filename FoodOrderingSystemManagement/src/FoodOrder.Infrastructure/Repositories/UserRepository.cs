using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using FoodOrder.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());

    public async Task<User?> GetByIdWithRoleAsync(int id) =>
        await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<IReadOnlyList<User>> GetAllWithRolesAsync(int? organizationId = null)
    {
        var query = _context.Users.Include(u => u.Role).Include(u => u.Branch).AsQueryable();
        if (organizationId.HasValue)
            query = query.Where(u => u.OrganizationId == organizationId.Value);
        else
            query = query.Where(u => u.OrganizationId != null); // exclude SuperAdmin from unscoped calls
        return await query.OrderBy(u => u.Role.RoleName).ThenBy(u => u.Name).ToListAsync();
    }

    public Task<int> CountByOrgAsync(int organizationId) =>
        _context.Users.CountAsync(u => u.OrganizationId == organizationId && u.IsActive);

    public async Task<User?> GetByResetTokenAsync(string token) =>
        await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token &&
                                      u.PasswordResetTokenExpiry > DateTime.UtcNow);

    public async Task DeleteUserSafeAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return;

        _context.Users.Remove(user);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // The account has real activity tied to it (orders, attendance, salary
            // records, etc.) — hard-deleting would orphan that data. Deactivate instead.
            throw new AppException(
                "Cannot delete this user because they have associated records (orders, attendance, etc.). Deactivate the account instead.",
                400);
        }
    }
}
