using Chatbot.Application.Auth;
using Chatbot.Application.Common.Exceptions;
using Chatbot.Application.Common.Interfaces;
using Chatbot.Domain.Entities;
using Moq;
using Xunit;

namespace Chatbot.Tests.Application;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IUserSettingsRepository> _userSettingsRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _userSettingsRepository.Object,
            _passwordHasher.Object,
            _tokenService.Object);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsConflict_WhenEmailAlreadyExists()
    {
        _userRepository.Setup(r => r.EmailExistsAsync("taken@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new RegisterRequest("Jane", "Doe", "taken@example.com", "Password123");

        await Assert.ThrowsAsync<ConflictAppException>(() => _sut.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_HashesPasswordAndIssuesTokens_WhenEmailIsFree()
    {
        _userRepository.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _passwordHasher.Setup(p => p.Hash("Password123")).Returns("hashed-password");
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");
        _tokenService.Setup(t => t.GetRefreshTokenExpiry()).Returns(DateTime.UtcNow.AddDays(7));
        _tokenService.Setup(t => t.GetAccessTokenExpiry()).Returns(DateTime.UtcNow.AddHours(1));

        var request = new RegisterRequest("Jane", "Doe", "jane@example.com", "Password123");

        var result = await _sut.RegisterAsync(request);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal("jane@example.com", result.User.Email);
        _userRepository.Verify(r => r.AddAsync(It.Is<User>(u => u.PasswordHash == "hashed-password"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorized_WhenUserDoesNotExist()
    {
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() => _sut.LoginAsync(new LoginRequest("nobody@example.com", "whatever")));
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorized_WhenPasswordDoesNotMatch()
    {
        var user = new User { Email = "jane@example.com", PasswordHash = "hashed", IsActive = true };
        _userRepository.Setup(r => r.GetByEmailAsync("jane@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("wrong-password", "hashed")).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() => _sut.LoginAsync(new LoginRequest("jane@example.com", "wrong-password")));
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorized_WhenUserIsInactive()
    {
        var user = new User { Email = "jane@example.com", PasswordHash = "hashed", IsActive = false };
        _userRepository.Setup(r => r.GetByEmailAsync("jane@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() => _sut.LoginAsync(new LoginRequest("jane@example.com", "Password123")));
    }
}
