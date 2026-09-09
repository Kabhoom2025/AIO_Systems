using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Infrastructure.Data;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Integration;

/// <summary>Mirrors NovaERP.Tests' WebApplicationFactory + "Testing" environment pattern: the
/// InMemory database fallback in Program.cs kicks in automatically for this environment.</summary>
public class AuthFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    // The API serializes enums as their string names (see Program.cs's AddJsonOptions), so the
    // test client's own ReadFromJsonAsync calls need the same converter — HttpContentJsonExtensions
    // otherwise falls back to System.Text.Json's numeric-enum default and every DTO with an enum
    // member (UserDto.Status, etc.) fails to deserialize.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly WebApplicationFactory<Program> _factory;

    public AuthFlowTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IEmailSender>();
                services.AddScoped<IEmailSender, CapturingEmailSender>();
            });
        });

    private async Task EnsureCreatedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProjectFlowDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task Register_Then_Login_Then_Refresh_Then_Logout_Full_Cycle_Succeeds()
    {
        await EnsureCreatedAsync();
        var client = _factory.CreateClient();
        var email = $"cycle-{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email, password = "Passw0rd!", firstName = "Cycle", lastName = "Test"
        });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        var registered = await registerResponse.Content.ReadFromJsonAsync<AuthResultDto>(JsonOptions);
        Assert.NotNull(registered);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Passw0rd!" });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loggedIn = await loginResponse.Content.ReadFromJsonAsync<AuthResultDto>(JsonOptions);
        Assert.NotNull(loggedIn);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loggedIn!.AccessToken);
        var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        var me = await meResponse.Content.ReadFromJsonAsync<CurrentUserDto>(JsonOptions);
        Assert.Equal(email, me!.Email);

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = loggedIn.RefreshToken });
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthResultDto>(JsonOptions);
        Assert.NotNull(refreshed);
        Assert.NotEqual(loggedIn.RefreshToken, refreshed!.RefreshToken);

        var logoutResponse = await client.PostAsJsonAsync("/api/auth/logout", new { refreshToken = refreshed.RefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        // The now-revoked refresh token must no longer work.
        var reuseResponse = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = refreshed.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);
    }

    [Fact]
    public async Task Register_Then_VerifyEmail_Happy_Path_Succeeds()
    {
        await EnsureCreatedAsync();
        var client = _factory.CreateClient();
        var email = $"verify-{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email, password = "Passw0rd!", firstName = "Verify", lastName = "Test"
        });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        var registered = await registerResponse.Content.ReadFromJsonAsync<AuthResultDto>(JsonOptions);

        var sentEmail = CapturingEmailSender.Sent.Single(e => e.To == email && e.Subject.Contains("Verify"));
        var token = Regex.Match(sentEmail.Body, @"code is: (\S+)").Groups[1].Value;
        Assert.NotEmpty(token);

        var verifyResponse = await client.PostAsJsonAsync("/api/auth/verify-email", new { userId = registered!.User.Id, token });
        Assert.Equal(HttpStatusCode.NoContent, verifyResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProjectFlowDbContext>();
        var user = await db.Users.FindAsync(registered.User.Id);
        Assert.True(user!.IsEmailVerified);
    }
}
