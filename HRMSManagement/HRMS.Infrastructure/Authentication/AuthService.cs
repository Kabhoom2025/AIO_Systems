using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HRMS.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private const int MaxFailedLogins = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly HrmsDbContext _ctx;
    private readonly IConfiguration _config;

    public AuthService(HrmsDbContext ctx, IConfiguration config)
    {
        _ctx = ctx;
        _config = config;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto, string? ipAddress, string? userAgent)
    {
        var user = await _ctx.Users
            .Include(u => u.Organization)
            .Include(u => u.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

        if (user == null)
        {
            await RecordLoginAsync(null, dto.Email, false, ipAddress, userAgent, "Unknown email or inactive user");
            return null;
        }

        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
        {
            await RecordLoginAsync(user.Id, dto.Email, false, ipAddress, userAgent, "Account locked");
            return null;
        }

        if (!PasswordHasher.Verify(dto.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= MaxFailedLogins)
            {
                user.LockedUntil = DateTime.UtcNow.Add(LockoutDuration);
                user.FailedLoginCount = 0;
            }
            await RecordLoginAsync(user.Id, dto.Email, false, ipAddress, userAgent, "Invalid password");
            return null;
        }

        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.LastLoginAt = DateTime.UtcNow;

        var refreshToken = new RefreshToken
        {
            UserId    = user.Id,
            Token     = GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenDays())
        };
        _ctx.RefreshTokens.Add(refreshToken);
        await RecordLoginAsync(user.Id, dto.Email, true, ipAddress, userAgent, null);

        return BuildLoginResponse(user, refreshToken.Token);
    }

    public async Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var stored = await _ctx.RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.Organization)
            .Include(t => t.User).ThenInclude(u => u.Role)
                .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (stored == null || !stored.IsActive || !stored.User.IsActive)
            return null;

        // Rotate: revoke old token, issue a new one
        var newToken = new RefreshToken
        {
            UserId    = stored.UserId,
            Token     = GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenDays())
        };
        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByToken = newToken.Token;
        _ctx.RefreshTokens.Add(newToken);
        await _ctx.SaveChangesAsync();

        return BuildLoginResponse(stored.User, newToken.Token, save: false);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var stored = await _ctx.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (stored == null) return;
        stored.RevokedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found");

        if (!PasswordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect.");

        ValidatePasswordPolicy(dto.NewPassword);

        user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
        user.UpdatedDate = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }

    public async Task<string?> ForgotPasswordAsync(string email)
    {
        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        if (user == null) return null; // do not reveal whether the email exists

        var token = new PasswordResetToken
        {
            UserId    = user.Id,
            Token     = GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        _ctx.PasswordResetTokens.Add(token);
        await _ctx.SaveChangesAsync();
        return token.Token;
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var token = await _ctx.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == dto.Token);

        if (token == null || token.IsUsed || token.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Reset token is invalid or expired.");

        ValidatePasswordPolicy(dto.NewPassword);

        token.User.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
        token.User.UpdatedDate = DateTime.UtcNow;
        token.IsUsed = true;
        await _ctx.SaveChangesAsync();
    }

    public async Task<List<LoginHistoryDto>> GetLoginHistoryAsync(int orgId, int take = 100)
    {
        return await _ctx.LoginHistories
            .Where(l => l.UserId == null || _ctx.Users.Any(u => u.Id == l.UserId && u.OrganizationId == orgId))
            .OrderByDescending(l => l.CreatedDate)
            .Take(take)
            .Select(l => new LoginHistoryDto
            {
                Id            = l.Id,
                Email         = l.Email,
                Success       = l.Success,
                IpAddress     = l.IpAddress,
                UserAgent     = l.UserAgent,
                FailureReason = l.FailureReason,
                CreatedDate   = l.CreatedDate
            })
            .ToListAsync();
    }

    private LoginResponseDto BuildLoginResponse(User user, string refreshToken, bool save = true)
    {
        if (save) _ctx.SaveChanges();

        var permissionKeys = user.Role.RolePermissions.Select(rp => rp.Permission.Key).ToList();

        var jwt = _config.GetSection("JwtSettings");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("organizationId", user.OrganizationId.ToString()),
            new Claim("branchId", user.BranchId?.ToString() ?? string.Empty),
            new Claim("employeeId", user.EmployeeId?.ToString() ?? string.Empty),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.Name),
            new Claim("permissions", string.Join(',', permissionKeys)),
            new Claim("tokenType", "staff")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var minutes = int.TryParse(jwt["AccessTokenMinutes"], out var m) ? m : 480;

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: creds);

        return new LoginResponseDto
        {
            Token            = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken     = refreshToken,
            UserId           = user.Id,
            EmployeeId       = user.EmployeeId,
            Name             = user.Name,
            Email            = user.Email,
            Role             = user.Role.Name,
            OrganizationId   = user.OrganizationId,
            OrganizationName = user.Organization.Name,
            Permissions      = permissionKeys
        };
    }

    private async Task RecordLoginAsync(int? userId, string email, bool success,
        string? ipAddress, string? userAgent, string? failureReason)
    {
        _ctx.LoginHistories.Add(new LoginHistory
        {
            UserId        = userId,
            Email         = email,
            Success       = success,
            IpAddress     = ipAddress,
            UserAgent     = userAgent,
            FailureReason = failureReason
        });
        await _ctx.SaveChangesAsync();
    }

    private static void ValidatePasswordPolicy(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new InvalidOperationException("Password must be at least 8 characters.");
        if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
            throw new InvalidOperationException("Password must contain upper case, lower case and a digit.");
    }

    private int GetRefreshTokenDays() =>
        int.TryParse(_config.GetSection("JwtSettings")["RefreshTokenDays"], out var d) ? d : 7;

    private static string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
