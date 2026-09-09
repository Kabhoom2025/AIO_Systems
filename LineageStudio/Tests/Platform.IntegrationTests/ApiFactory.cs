using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Platform.Application.Auth;
using Platform.Domain.Entities;
using Platform.Infrastructure.Persistence;

namespace Platform.IntegrationTests;

/// <summary>
/// Points the API at a dedicated LineageStudioTestDB (never LineageStudioDB) and runs real
/// migrations against it. Requires a reachable PostgreSQL instance with the same
/// postgres/1234 credentials the rest of the repo's local dev setup uses.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string TestConnectionString =
        "Host=localhost;Port=5432;Database=LineageStudioTestDB;Username=postgres;Password=1234";

    public const string TestUserEmail = "test@example.com";
    public const string TestUserPassword = "Test@12345";

    /// <summary>Matches the enum-as-string convention Platform.Api's controllers serialize
    /// with - the test HttpClient otherwise has no idea DataType comes back as "Uuid" and not 0.</summary>
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
                ["Seed:Enabled"] = "false",
            });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = TestUserEmail,
            PasswordHash = hasher.Hash(TestUserPassword),
            DisplayName = "Test User",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    /// <summary>A client pre-authenticated as the seeded test user - what almost every test
    /// should use, since the API now requires a bearer token on every endpoint except
    /// /api/auth/login. Mints the token locally (same signing key the running app validates
    /// against, read from its own configuration) rather than a real HTTP login round-trip,
    /// since most tests care about the feature under test, not re-proving login works - that's
    /// what AuthControllerTests is for.</summary>
    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MintTestToken());
        return client;
    }

    public string MintTestToken(Guid? userId = null)
    {
        var jwt = Services.GetRequiredService<IConfiguration>().GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, (userId ?? Guid.NewGuid()).ToString()),
            new Claim(JwtRegisteredClaimNames.Email, TestUserEmail),
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"], audience: jwt["Audience"],
            claims: claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
