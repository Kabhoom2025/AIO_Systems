using FoodOrder.Application.DTOs.User;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ILicenseRepository _licenseRepository;
    private readonly IBranchRepository _branchRepository;

    public UserService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        ILicenseRepository licenseRepository,
        IBranchRepository branchRepository)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _licenseRepository = licenseRepository;
        _branchRepository = branchRepository;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(int? organizationId)
    {
        var users = await _userRepository.GetAllWithRolesAsync(organizationId);
        return users.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto, int? organizationId)
    {
        var normalizedEmail = dto.Email.ToLower().Trim();

        var existing = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (existing is not null)
            throw new AppException($"A user with email '{dto.Email}' already exists.");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            throw new AppException("Password must be at least 6 characters.");

        // Enforce license user limit
        if (organizationId.HasValue)
        {
            var license = await _licenseRepository.GetByOrgAsync(organizationId.Value);
            if (license != null)
            {
                var currentCount = await _userRepository.CountByOrgAsync(organizationId.Value);
                if (currentCount >= license.MaxUsers)
                    throw new ForbiddenException(
                        $"User limit reached. Your {license.Plan} plan allows a maximum of {license.MaxUsers} active users. " +
                        "Please contact your system administrator to upgrade your plan.");
            }
        }

        await ValidateBranchAsync(dto.BranchId, organizationId);

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordService.HashPassword(dto.Password),
            RoleId = dto.RoleId,
            OrganizationId = organizationId,
            BranchId = dto.BranchId,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            ProfileImage = dto.ProfileImage,
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Reload to get the Role navigation populated.
        var created = await _userRepository.GetByIdWithRoleAsync(user.Id)
            ?? throw new AppException("Failed to reload created user.");

        return MapToDto(created);
    }

    public async Task<UserDto> UpdateUserAsync(int userId, UpdateStaffUserDto dto)
    {
        var user = await _userRepository.GetByIdWithRoleAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var normalizedEmail = dto.Email.ToLower().Trim();
        if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (existing is not null)
                throw new AppException($"A user with email '{dto.Email}' already exists.");
        }

        await ValidateBranchAsync(dto.BranchId, user.OrganizationId);

        user.Name = dto.Name.Trim();
        user.Email = normalizedEmail;
        user.RoleId = dto.RoleId;
        user.BranchId = dto.BranchId;
        user.ProfileImage = dto.ProfileImage;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        var updated = await _userRepository.GetByIdWithRoleAsync(user.Id)
            ?? throw new AppException("Failed to reload updated user.");

        return MapToDto(updated);
    }

    public async Task ToggleActiveAsync(int userId, int requestingUserId)
    {
        if (userId == requestingUserId)
            throw new AppException("You cannot deactivate your own account.");

        var user = await _userRepository.GetByIdWithRoleAsync(userId)
            ?? throw new NotFoundException("User", userId);

        user.IsActive = !user.IsActive;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int userId, int requestingUserId)
    {
        if (userId == requestingUserId)
            throw new AppException("You cannot delete your own account.");

        _ = await _userRepository.GetByIdWithRoleAsync(userId)
            ?? throw new NotFoundException("User", userId);

        await _userRepository.DeleteUserSafeAsync(userId);
    }

    /// <summary>Guards against assigning a branch that belongs to a different organization.</summary>
    private async Task ValidateBranchAsync(int? branchId, int? organizationId)
    {
        if (!branchId.HasValue) return;

        var branch = await _branchRepository.GetByIdAsync(branchId.Value)
            ?? throw new AppException("Branch not found.", 404);

        if (organizationId.HasValue && branch.OrganizationId != organizationId.Value)
            throw new AppException("That branch does not belong to your organization.", 400);
    }

    private static UserDto MapToDto(User u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email,
        RoleId = u.RoleId,
        RoleName = u.Role?.RoleName ?? string.Empty,
        BranchId = u.BranchId,
        BranchName = u.Branch?.Name,
        IsActive = u.IsActive,
        CreatedDate = u.CreatedDate,
        ProfileImage = u.ProfileImage,
    };
}
