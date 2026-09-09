using System.IdentityModel.Tokens.Jwt;
using Chatbot.Domain.Entities;
using Chatbot.Domain.Enums;
using Chatbot.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Xunit;

namespace Chatbot.Tests.Infrastructure;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "unit-test-signing-secret-at-least-32-characters-long",
            Issuer = "ChatbotAI.Tests",
            Audience = "ChatbotAI.Tests.Client",
            ExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        });
        _sut = new JwtTokenService(options);
    }

    [Fact]
    public void GenerateAccessToken_ProducesTokenWithExpectedClaims()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "jane@example.com",
            FirstName = "Jane",
            LastName = "Doe",
            Role = UserRole.Admin
        };

        var token = _sut.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("ChatbotAI.Tests", jwt.Issuer);
        Assert.Contains(jwt.Claims, c => c.Type == "email" && c.Value == "jane@example.com");
        Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateRefreshToken_ProducesUniqueValues()
    {
        var first = _sut.GenerateRefreshToken();
        var second = _sut.GenerateRefreshToken();

        Assert.NotEqual(first, second);
        Assert.False(string.IsNullOrWhiteSpace(first));
    }
}
