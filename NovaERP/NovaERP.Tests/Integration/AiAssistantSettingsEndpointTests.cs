using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NovaERP.Application.DTOs;
using NovaERP.Infrastructure.Data;
using Xunit;

namespace NovaERP.Tests.Integration;

/// <summary>Exercises the DB-backed, UI-editable AI provider settings — mirrors
/// NotificationChannelSettings' conventions: GET never returns a raw key (only a computed
/// IsConfigured flag), and PUT with a blank key means "keep the existing key unchanged". Also
/// covers the two-provider (Anthropic/Groq) shape: each provider's key is tracked independently
/// of which Provider is currently selected.</summary>
public class AiAssistantSettingsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AiAssistantSettingsEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

    private async Task EnsureSeededAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaErpDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedData.SeedAsync(db);
    }

    private async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return body!.Token;
    }

    [Fact]
    public async Task Get_With_No_Row_Yet_Returns_NotConfigured()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var response = await client.GetAsync("/api/ai-assistant-settings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<AiAssistantSettingsDto>();
        Assert.False(dto!.IsConfigured);
    }

    [Fact]
    public async Task Put_Sets_Key_Then_Get_Shows_Configured_And_Never_Echoes_Key()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var putResponse = await client.PutAsJsonAsync("/api/ai-assistant-settings",
            new UpdateAiAssistantSettingsDto { AnthropicApiKey = "sk-ant-test-key", AnthropicModel = "claude-sonnet-4-5" });
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var putBody = await putResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("sk-ant-test-key", putBody);

        var getResponse = await client.GetAsync("/api/ai-assistant-settings");
        var dto = await getResponse.Content.ReadFromJsonAsync<AiAssistantSettingsDto>();
        Assert.True(dto!.IsConfigured);

        var getBody = await getResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("sk-ant-test-key", getBody);
    }

    [Fact]
    public async Task Put_With_Blank_Key_Leaves_Existing_Key_Unchanged()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        await client.PutAsJsonAsync("/api/ai-assistant-settings",
            new UpdateAiAssistantSettingsDto { AnthropicApiKey = "sk-ant-original-key", AnthropicModel = "claude-sonnet-4-5" });

        var secondPut = await client.PutAsJsonAsync("/api/ai-assistant-settings",
            new UpdateAiAssistantSettingsDto { AnthropicApiKey = "", AnthropicModel = "claude-sonnet-4-5" });
        Assert.Equal(HttpStatusCode.OK, secondPut.StatusCode);

        var dto = await secondPut.Content.ReadFromJsonAsync<AiAssistantSettingsDto>();
        Assert.True(dto!.IsConfigured);
    }

    [Fact]
    public async Task Employee_Can_View_But_Cannot_Edit()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var getResponse = await client.GetAsync("/api/ai-assistant-settings");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var putResponse = await client.PutAsJsonAsync("/api/ai-assistant-settings",
            new UpdateAiAssistantSettingsDto { AnthropicApiKey = "sk-ant-test-key", AnthropicModel = "claude-sonnet-4-5" });
        Assert.Equal(HttpStatusCode.Forbidden, putResponse.StatusCode);
    }

    [Fact]
    public async Task Default_Provider_Is_Anthropic()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var response = await client.GetAsync("/api/ai-assistant-settings");
        var dto = await response.Content.ReadFromJsonAsync<AiAssistantSettingsDto>();

        Assert.Equal("Anthropic", dto!.Provider);
    }

    [Fact]
    public async Task Groq_Key_Configures_Groq_Without_Needing_An_Anthropic_Key()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var putResponse = await client.PutAsJsonAsync("/api/ai-assistant-settings",
            new UpdateAiAssistantSettingsDto { Provider = "Groq", GroqApiKey = "gsk-test-key", GroqModel = "llama-3.3-70b-versatile" });
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var putBody = await putResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("gsk-test-key", putBody);

        var dto = await putResponse.Content.ReadFromJsonAsync<AiAssistantSettingsDto>();
        Assert.Equal("Groq", dto!.Provider);
        Assert.True(dto.IsConfigured);
    }

    [Fact]
    public async Task Switching_Provider_Back_Tracks_Each_Providers_Key_Independently()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        // Configure Groq only.
        await client.PutAsJsonAsync("/api/ai-assistant-settings",
            new UpdateAiAssistantSettingsDto { Provider = "Groq", GroqApiKey = "gsk-test-key" });

        // Switch back to Anthropic without ever setting an Anthropic key.
        var switchBack = await client.PutAsJsonAsync("/api/ai-assistant-settings",
            new UpdateAiAssistantSettingsDto { Provider = "Anthropic" });
        var dto = await switchBack.Content.ReadFromJsonAsync<AiAssistantSettingsDto>();

        Assert.Equal("Anthropic", dto!.Provider);
        Assert.False(dto.IsConfigured);
    }

    [Fact]
    public async Task Get_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/ai-assistant-settings");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
