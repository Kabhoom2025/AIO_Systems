using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

/// <summary>
/// Extends the generic repository with user-specific queries that require
/// navigation property loading or custom lookup logic.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithRoleAsync(int id);
    Task<IReadOnlyList<User>> GetAllWithRolesAsync(int? organizationId = null);
    Task<int> CountByOrgAsync(int organizationId);
    Task<User?> GetByResetTokenAsync(string token);

    /// <summary>Hard-deletes a user, translating a dependent-data conflict into a friendly AppException.</summary>
    Task DeleteUserSafeAsync(int id);
}
