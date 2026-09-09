using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FlowSphere.Infrastructure.Authentication;

/// <summary>
/// Issues JWTs signed with the SAME SecretKey/Issuer/Audience already shared across
/// NovaERP/HRMS/FoodOrder/Pharmacy in this repo (copied verbatim in appsettings.json), so a
/// token minted by any sibling service's login is structurally valid here and vice versa.
/// Claims: sub (userId), organizationId, permissions (comma-joined), name - matching the
/// existing cross-service claim contract.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateToken(User user, Role role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new("organizationId", user.OrganizationId.ToString()),
            new("permissions", role.Permissions),
            new("roleName", role.Name)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
