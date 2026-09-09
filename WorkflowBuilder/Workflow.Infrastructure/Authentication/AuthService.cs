using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Workflow.Application.DTOs;
using Workflow.Application.Interfaces;
using Workflow.Infrastructure.Data;

namespace Workflow.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private readonly WorkflowDbContext _ctx;
    private readonly IConfiguration _config;
    private readonly IPasswordHasher _hasher;

    public AuthService(WorkflowDbContext ctx, IConfiguration config, IPasswordHasher hasher)
    {
        _ctx = ctx;
        _config = config;
        _hasher = hasher;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);
        if (user == null || !_hasher.Verify(dto.Password, user.PasswordHash))
            return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();

        var jwt = _config.GetSection("JwtSettings");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("organizationId", user.OrganizationId.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
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
            Token          = new JwtSecurityTokenHandler().WriteToken(token),
            UserId         = user.Id,
            Name           = user.Name,
            Email          = user.Email,
            Role           = user.Role,
            OrganizationId = user.OrganizationId
        };
    }
}
