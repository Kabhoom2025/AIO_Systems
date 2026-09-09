using System.Security.Cryptography;
using FoodOrder.Application.DTOs.Auth;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly ILicenseRepository _licenseRepository;
    private readonly IEmailService _emailService;
    private readonly IPlatformModuleService _platformModuleService;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordService passwordService,
        ILicenseRepository licenseRepository,
        IEmailService emailService,
        IPlatformModuleService platformModuleService)
    {
        _userRepository        = userRepository;
        _tokenService          = tokenService;
        _passwordService       = passwordService;
        _licenseRepository     = licenseRepository;
        _emailService          = emailService;
        _platformModuleService = platformModuleService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email)
            ?? throw new AppException("Invalid email or password.", statusCode: 401);

        if (!user.IsActive)
            throw new AppException("Your account has been deactivated. Please contact the administrator.", statusCode: 401);

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw new AppException("Invalid email or password.", statusCode: 401);

        // License check — SuperAdmin (no OrganizationId) is exempt
        if (user.OrganizationId.HasValue)
        {
            var license = await _licenseRepository.GetByOrgAsync(user.OrganizationId.Value);

            if (license == null)
                throw new ForbiddenException(
                    "No license has been assigned to your organization. Please contact your system administrator.");

            if (license.Status == "Expired" || license.ExpiryDate < DateTime.UtcNow)
                throw new ForbiddenException(
                    "Your organization's license has expired. Please contact your system administrator to renew.");
        }

        var tokenResult = _tokenService.GenerateToken(user);

        var response = new LoginResponseDto
        {
            UserId = user.Id,
            RoleId = user.RoleId,
            Name = user.Name,
            Email = user.Email,
            RoleName = user.Role.RoleName,
            Token = tokenResult.Token,
            ExpiresAt = tokenResult.ExpiresAt
        };
        response.OrganizationId   = user.OrganizationId;
        response.OrganizationName = user.Organization?.Name;
        response.IsSuperAdmin     = user.RoleId == 1 && !user.OrganizationId.HasValue;
        response.ProfileImage     = user.ProfileImage;

        if (user.OrganizationId.HasValue)
            response.EnabledModules = await _platformModuleService.GetEnabledModuleKeysForOrgAsync(user.OrganizationId.Value);

        return response;
    }

    public async Task<LoginResponseDto> SsoLoginAsync(string email, string providerName)
    {
        var user = await _userRepository.GetByEmailAsync(email)
            ?? throw new AppException(
                $"No account found for '{email}'. Ask your administrator to create one.", statusCode: 401);

        if (!user.IsActive)
            throw new AppException("Your account has been deactivated. Contact the administrator.", statusCode: 401);

        if (user.OrganizationId.HasValue)
        {
            var license = await _licenseRepository.GetByOrgAsync(user.OrganizationId.Value);
            if (license == null)
                throw new ForbiddenException("No license assigned to your organization.");
            if (license.Status == "Expired" || license.ExpiryDate < DateTime.UtcNow)
                throw new ForbiddenException("Your organization's license has expired.");
        }

        var tokenResult = _tokenService.GenerateToken(user);
        var response = new LoginResponseDto
        {
            UserId   = user.Id,
            RoleId   = user.RoleId,
            Name     = user.Name,
            Email    = user.Email,
            RoleName = user.Role.RoleName,
            Token    = tokenResult.Token,
            ExpiresAt = tokenResult.ExpiresAt,
        };
        response.OrganizationId   = user.OrganizationId;
        response.OrganizationName = user.Organization?.Name;
        response.IsSuperAdmin     = user.RoleId == 1 && !user.OrganizationId.HasValue;
        response.ProfileImage     = user.ProfileImage;

        if (user.OrganizationId.HasValue)
            response.EnabledModules = await _platformModuleService.GetEnabledModuleKeysForOrgAsync(user.OrganizationId.Value);

        return response;
    }

    public async Task<string> ForgotPasswordAsync(string email, string resetBaseUrl)
    {
        // Always return success to prevent email enumeration — silently skip unknown emails.
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive)
            return "If that email is registered, a reset link has been sent.";

        var token  = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
                         .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        var expiry = DateTime.UtcNow.AddMinutes(30);

        user.PasswordResetToken      = token;
        user.PasswordResetTokenExpiry = expiry;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        var resetLink = $"{resetBaseUrl.TrimEnd('/')}/auth/reset-password?token={Uri.EscapeDataString(token)}";
        await _emailService.SendPasswordResetAsync(user.Email, user.Name, resetLink);

        return "If that email is registered, a reset link has been sent.";
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            throw new AppException("Passwords do not match.", statusCode: 400);

        var user = await _userRepository.GetByResetTokenAsync(dto.Token)
            ?? throw new AppException("This reset link is invalid or has expired. Please request a new one.", statusCode: 400);

        user.PasswordHash             = _passwordService.HashPassword(dto.NewPassword);
        user.PasswordResetToken       = null;
        user.PasswordResetTokenExpiry = null;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }
}
