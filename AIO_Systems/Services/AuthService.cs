using AIO_Systems.Data;
using AIO_Systems.DTOs.Auth;
using AIO_Systems.Services.Interfaces;
using AIO_Systems.Shared;
using Microsoft.EntityFrameworkCore;

namespace AIO_Systems.Services;

public class AuthService(
    AppDbContext db,
    ITokenService tokenService,
    IPasswordService passwordService,
    IPlatformModuleService platformModuleService) : IAuthService
{
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await db.Users
            .Include(u => u.Role)
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == request.Email)
            ?? throw new AppException("Invalid email or password.", statusCode: 401);

        if (!user.IsActive)
            throw new AppException("Your account has been deactivated. Please contact the administrator.", statusCode: 401);

        if (!passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw new AppException("Invalid email or password.", statusCode: 401);

        // License check — SuperAdmin (no OrganizationId) is exempt
        if (user.OrganizationId.HasValue)
        {
            var license = await db.Licenses
                .FirstOrDefaultAsync(l => l.OrganizationId == user.OrganizationId.Value);

            if (license == null)
                throw new ForbiddenException(
                    "No license has been assigned to your organization. Please contact your system administrator.");

            if (license.Status == "Expired" || license.ExpiryDate < DateTime.UtcNow)
                throw new ForbiddenException(
                    "Your organization's license has expired. Please contact your system administrator to renew.");
        }

        var tokenResult = tokenService.GenerateToken(user);

        var response = new LoginResponseDto
        {
            UserId = user.Id,
            RoleId = user.RoleId,
            Name = user.Name,
            Email = user.Email,
            RoleName = user.Role.RoleName,
            Token = tokenResult.Token,
            ExpiresAt = tokenResult.ExpiresAt,
            OrganizationId = user.OrganizationId,
            OrganizationName = user.Organization?.Name,
            IsSuperAdmin = user.RoleId == 1 && !user.OrganizationId.HasValue,
            ProfileImage = user.ProfileImage,
        };

        if (user.OrganizationId.HasValue)
            response.EnabledModules = await platformModuleService.GetEnabledModuleKeysForOrgAsync(user.OrganizationId.Value);

        return response;
    }
}
