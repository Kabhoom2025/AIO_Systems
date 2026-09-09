using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private readonly PharmacyDbContext _ctx;
    private readonly IConfiguration _config;

    public AuthService(PharmacyDbContext ctx, IConfiguration config)
    {
        _ctx = ctx;
        _config = config;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _ctx.Users
            .Include(u => u.Organization)
            .Include(u => u.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

        if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
            return null;

        var permissionKeys = string.Join(',', user.Role.RolePermissions.Select(rp => rp.Permission.Key));

        var jwt = _config.GetSection("JwtSettings");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("organizationId", user.OrganizationId.ToString()),
            new Claim("branchId", user.BranchId?.ToString() ?? string.Empty),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.Name),
            new Claim("permissions", permissionKeys),
            new Claim("tokenType", "staff")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new LoginResponseDto
        {
            Token            = new JwtSecurityTokenHandler().WriteToken(token),
            UserId           = user.Id,
            Name             = user.Name,
            Email            = user.Email,
            Role             = user.Role.Name,
            OrganizationId   = user.OrganizationId,
            OrganizationName = user.Organization.Name
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found");

        if (!PasswordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect.");

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            throw new InvalidOperationException("New password must be at least 6 characters.");

        user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
        user.UpdatedDate = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }
}
