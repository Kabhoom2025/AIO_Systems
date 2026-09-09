using LinkShield.Application.DTOs.Auth;
using LinkShield.Domain.Entities;
using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.Authentication;
using LinkShield.Infrastructure.Persistence;
using LinkShield.Tests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LinkShield.Tests.Authentication;

public class AuthServiceTests
{
    private const string ValidPassword = "Password@123";

    private static LinkShieldDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LinkShieldDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LinkShieldDbContext(options);
    }

    private static IConfiguration CreateConfig() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "LinkShield@SuperSecret#Key$2026!MustBe32CharsOrMore",
            ["JwtSettings:Issuer"] = "LinkShieldAPI",
            ["JwtSettings:Audience"] = "LinkShieldClient",
            ["JwtSettings:AccessTokenMinutes"] = "60",
            ["JwtSettings:RefreshTokenDays"] = "7"
        })
        .Build();

    private static async Task SeedDefaultRoleAsync(LinkShieldDbContext ctx)
    {
        ctx.Roles.Add(new Role { Name = SystemRole.User, Description = "User" });
        await ctx.SaveChangesAsync();
    }

    private static AuthService CreateAuthService(LinkShieldDbContext ctx, FakeEmailSender emailSender) =>
        new(ctx, new PasswordHasherAdapter(), new JwtTokenGenerator(CreateConfig()), emailSender);

    [Fact]
    public async Task RegisterAsync_CreatesUser_AssignsDefaultRole_ReturnsTokens()
    {
        var ctx = CreateContext();
        await SeedDefaultRoleAsync(ctx);
        var emailSender = new FakeEmailSender();
        var service = CreateAuthService(ctx, emailSender);

        var result = await service.RegisterAsync(new RegisterRequestDto("new@user.local", ValidPassword, "New", "User"));

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.Equal("new@user.local", result.Email);
        Assert.Contains("User", result.Roles);
        Assert.Single(emailSender.SentEmails);
        Assert.Equal(1, await ctx.Users.CountAsync());
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenEmailAlreadyExists()
    {
        var ctx = CreateContext();
        await SeedDefaultRoleAsync(ctx);
        var service = CreateAuthService(ctx, new FakeEmailSender());
        await service.RegisterAsync(new RegisterRequestDto("dupe@user.local", ValidPassword, "A", "B"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(new RegisterRequestDto("dupe@user.local", ValidPassword, "C", "D")));
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokens_ForValidCredentials()
    {
        var ctx = CreateContext();
        await SeedDefaultRoleAsync(ctx);
        var service = CreateAuthService(ctx, new FakeEmailSender());
        await service.RegisterAsync(new RegisterRequestDto("login@user.local", ValidPassword, "Log", "In"));

        var result = await service.LoginAsync(new LoginRequestDto("login@user.local", ValidPassword), "127.0.0.1");

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
    }

    [Fact]
    public async Task LoginAsync_Throws_ForInvalidPassword()
    {
        var ctx = CreateContext();
        await SeedDefaultRoleAsync(ctx);
        var service = CreateAuthService(ctx, new FakeEmailSender());
        await service.RegisterAsync(new RegisterRequestDto("baduser@user.local", ValidPassword, "Bad", "User"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequestDto("baduser@user.local", "WrongPassword@1"), "127.0.0.1"));
    }

    [Fact]
    public async Task LoginAsync_LocksAccount_AfterFiveFailedAttempts()
    {
        var ctx = CreateContext();
        await SeedDefaultRoleAsync(ctx);
        var service = CreateAuthService(ctx, new FakeEmailSender());
        await service.RegisterAsync(new RegisterRequestDto("locked@user.local", ValidPassword, "Lock", "Ed"));

        for (var i = 0; i < 5; i++)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.LoginAsync(new LoginRequestDto("locked@user.local", "WrongPassword@1"), "127.0.0.1"));
        }

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequestDto("locked@user.local", ValidPassword), "127.0.0.1"));
        Assert.Contains("locked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RefreshTokenAsync_RotatesToken_AndRevokesOld()
    {
        var ctx = CreateContext();
        await SeedDefaultRoleAsync(ctx);
        var service = CreateAuthService(ctx, new FakeEmailSender());
        var registerResult = await service.RegisterAsync(new RegisterRequestDto("refresh@user.local", ValidPassword, "Re", "Fresh"));

        var refreshed = await service.RefreshTokenAsync(registerResult.RefreshToken, "127.0.0.1");

        Assert.NotEqual(registerResult.RefreshToken, refreshed.RefreshToken);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RefreshTokenAsync(registerResult.RefreshToken, "127.0.0.1"));
    }

    [Fact]
    public async Task ForgotPasswordAsync_DoesNotThrow_ForUnknownEmail()
    {
        var ctx = CreateContext();
        await SeedDefaultRoleAsync(ctx);
        var service = CreateAuthService(ctx, new FakeEmailSender());

        await service.ForgotPasswordAsync("nobody@nowhere.local");
    }

    [Fact]
    public async Task ResetPasswordAsync_UpdatesPassword_WithValidToken()
    {
        var ctx = CreateContext();
        await SeedDefaultRoleAsync(ctx);
        var emailSender = new FakeEmailSender();
        var service = CreateAuthService(ctx, emailSender);
        await service.RegisterAsync(new RegisterRequestDto("reset@user.local", ValidPassword, "Re", "Set"));
        await service.ForgotPasswordAsync("reset@user.local");

        var user = await ctx.Users.FirstAsync(u => u.Email == "reset@user.local");
        var token = user.PasswordResetToken!;

        await service.ResetPasswordAsync(new ResetPasswordRequestDto(token, "NewPassword@456"));

        var loginResult = await service.LoginAsync(new LoginRequestDto("reset@user.local", "NewPassword@456"), "127.0.0.1");
        Assert.False(string.IsNullOrWhiteSpace(loginResult.AccessToken));
    }

    [Fact]
    public async Task VerifyEmailAsync_MarksVerified_WithValidToken()
    {
        var ctx = CreateContext();
        await SeedDefaultRoleAsync(ctx);
        var service = CreateAuthService(ctx, new FakeEmailSender());
        await service.RegisterAsync(new RegisterRequestDto("verify@user.local", ValidPassword, "Ver", "Ify"));

        var user = await ctx.Users.FirstAsync(u => u.Email == "verify@user.local");
        await service.VerifyEmailAsync(user.EmailVerificationToken!);

        var updated = await ctx.Users.FirstAsync(u => u.Email == "verify@user.local");
        Assert.True(updated.EmailVerified);
    }
}
