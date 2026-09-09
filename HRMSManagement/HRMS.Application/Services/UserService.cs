using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;

    public UserService(IUserRepository repo, IPasswordHasher hasher)
    {
        _repo   = repo;
        _hasher = hasher;
    }

    public async Task<List<UserDto>> GetAllAsync(int orgId)
    {
        var users = await _repo.GetAllByOrgAsync(orgId);
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int orgId, int id)
    {
        var user = await _repo.GetByIdAsync(id);
        return user == null || user.OrganizationId != orgId ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(int orgId, CreateUserDto dto)
    {
        if (await _repo.ExistsByEmailAsync(dto.Email))
            throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");

        ValidatePassword(dto.Password);

        var user = new User
        {
            OrganizationId = orgId,
            Name           = dto.Name,
            Email          = dto.Email,
            PasswordHash   = _hasher.Hash(dto.Password),
            RoleId         = dto.RoleId,
            BranchId       = dto.BranchId,
            EmployeeId     = dto.EmployeeId,
            IsActive       = true
        };
        _repo.Add(user);
        await _repo.SaveChangesAsync();

        var created = await _repo.GetByIdAsync(user.Id);
        return MapToDto(created!);
    }

    public async Task<UserDto> UpdateAsync(int orgId, int id, UpdateUserDto dto)
    {
        var user = await GetOwnedUserAsync(orgId, id);

        user.Name        = dto.Name;
        user.RoleId      = dto.RoleId;
        user.BranchId    = dto.BranchId;
        user.EmployeeId  = dto.EmployeeId;
        user.IsActive    = dto.IsActive;
        user.UpdatedDate = DateTime.UtcNow;

        _repo.Update(user);
        await _repo.SaveChangesAsync();

        var updated = await _repo.GetByIdAsync(user.Id);
        return MapToDto(updated!);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var user = await GetOwnedUserAsync(orgId, id);

        // Soft delete: deactivate the account, keep the row for auditing.
        user.IsActive    = false;
        user.UpdatedDate = DateTime.UtcNow;

        _repo.Update(user);
        await _repo.SaveChangesAsync();
    }

    public async Task ResetPasswordAsync(int orgId, int id, ResetUserPasswordDto dto)
    {
        var user = await GetOwnedUserAsync(orgId, id);

        ValidatePassword(dto.NewPassword);

        user.PasswordHash = _hasher.Hash(dto.NewPassword);
        user.UpdatedDate  = DateTime.UtcNow;

        _repo.Update(user);
        await _repo.SaveChangesAsync();
    }

    private async Task<User> GetOwnedUserAsync(int orgId, int id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null || user.OrganizationId != orgId)
            throw new KeyNotFoundException($"User {id} not found");
        return user;
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password) ||
            password.Length < 8 ||
            !password.Any(char.IsUpper) ||
            !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit))
            throw new InvalidOperationException(
                "Password must be at least 8 characters long and contain an uppercase letter, a lowercase letter and a digit.");
    }

    private static UserDto MapToDto(User u) => new()
    {
        Id           = u.Id,
        Name         = u.Name,
        Email        = u.Email,
        RoleId       = u.RoleId,
        RoleName     = u.Role?.Name ?? string.Empty,
        BranchId     = u.BranchId,
        BranchName   = u.Branch?.Name,
        EmployeeId   = u.EmployeeId,
        EmployeeName = u.Employee?.FullName,
        IsActive     = u.IsActive,
        LastLoginAt  = u.LastLoginAt,
        CreatedDate  = u.CreatedDate
    };
}
