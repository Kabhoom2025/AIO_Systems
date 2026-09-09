using System.Net;
using System.Net.Http.Json;
using Platform.Application.Auth;

namespace Platform.IntegrationTests;

public class AuthControllerTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(); // deliberately unauthenticated - this is what's under test
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task A_request_without_a_token_is_rejected()
    {
        var response = await _client.GetAsync("/api/applications");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_request_with_a_valid_token_succeeds()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _factory.MintTestToken());

        var response = await _client.GetAsync("/api/applications");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_with_the_real_seeded_user_returns_a_working_token()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(ApiFactory.TestUserEmail, ApiFactory.TestUserPassword));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrWhiteSpace(login!.Token));
        Assert.Equal(ApiFactory.TestUserEmail, login.Email);
        Assert.True(login.ExpiresAt > DateTimeOffset.UtcNow);

        // The token this real login endpoint just issued must actually work against a
        // protected endpoint - not just look like a JWT.
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login.Token);
        var protectedResponse = await _client.GetAsync("/api/applications");
        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
    }

    [Fact]
    public async Task Login_with_the_wrong_password_is_rejected()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(ApiFactory.TestUserEmail, "not-the-real-password"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_for_an_unknown_email_is_rejected()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("nobody@example.com", "whatever"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task The_health_check_does_not_require_a_token()
    {
        var response = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
