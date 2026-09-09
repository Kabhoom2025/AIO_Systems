using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NovaERP.Application.DTOs;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Authentication;
using NovaERP.Infrastructure.Data;
using Xunit;

namespace NovaERP.Tests.Authentication;

public class AuthServiceLoginTests
{
    private static NovaErpDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NovaErpDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NovaErpDbContext(options);
    }

    private static IConfiguration CreateConfig() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "FoodOrder@SuperSecret#Key$2025!MustBe32CharsOrMore",
            ["JwtSettings:Issuer"] = "FoodOrderAPI",
            ["JwtSettings:Audience"] = "FoodOrderClient",
            ["JwtSettings:AccessTokenMinutes"] = "480",
            ["JwtSettings:RefreshTokenDays"] = "7"
        })
        .Build();

    private static async Task<(NovaErpDbContext ctx, Organization org, Role role, User user)> SeedUserAsync(NovaErpDbContext ctx, string password)
    {
        var org = new Organization { Name = "Acme", Code = "ACME" };
        var role = new Role { Name = "Admin", IsSystemRole = true };
        var user = new User
        {
            Organization = org, Role = role, Name = "Test User",
            Email = "test@novaerp.local", PasswordHash = PasswordHasher.Hash(password), IsActive = true
        };
        ctx.Organizations.Add(org);
        ctx.Roles.Add(role);
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();
        return (ctx, org, role, user);
    }

    [Fact]
    public async Task LoginAsync_Returns_Token_For_Valid_Credentials()
    {
        var ctx = CreateContext();
        await SeedUserAsync(ctx, "Password@123");
        var service = new AuthService(ctx, CreateConfig());

        var result = await service.LoginAsync(new LoginDto { Email = "test@novaerp.local", Password = "Password@123" }, "127.0.0.1", "test-agent");

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.Token));
        Assert.Equal("test@novaerp.local", result.Email);
    }

    [Fact]
    public async Task LoginAsync_Returns_Null_For_Invalid_Password()
    {
        var ctx = CreateContext();
        await SeedUserAsync(ctx, "Password@123");
        var service = new AuthService(ctx, CreateConfig());

        var result = await service.LoginAsync(new LoginDto { Email = "test@novaerp.local", Password = "WrongPassword" }, null, null);

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_Locks_Account_After_Five_Failed_Attempts()
    {
        var ctx = CreateContext();
        var (_, _, _, user) = await SeedUserAsync(ctx, "Password@123");
        var service = new AuthService(ctx, CreateConfig());

        for (var i = 0; i < 5; i++)
            await service.LoginAsync(new LoginDto { Email = "test@novaerp.local", Password = "WrongPassword" }, null, null);

        var reloaded = await ctx.Users.FirstAsync(u => u.Id == user.Id);
        Assert.NotNull(reloaded.LockedUntil);
        Assert.True(reloaded.LockedUntil > DateTime.UtcNow);

        // Even the correct password should now fail while locked.
        var result = await service.LoginAsync(new LoginDto { Email = "test@novaerp.local", Password = "Password@123" }, null, null);
        Assert.Null(result);
    }
}
