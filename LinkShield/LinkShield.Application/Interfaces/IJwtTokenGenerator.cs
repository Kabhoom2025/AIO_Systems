using LinkShield.Domain.Entities;

namespace LinkShield.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user, IReadOnlyList<string> roles);
    string GenerateRefreshToken();
    int RefreshTokenLifetimeDays { get; }
}
