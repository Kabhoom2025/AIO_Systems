using System.Net;
using System.Text.Json;
using Chatbot.Application.Common.Interfaces;
using Chatbot.Infrastructure.Ai;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Chatbot.Tests.Infrastructure;

public class AiChatServiceFactoryTests
{
    private static AiOptions ServerDefaults() => new()
    {
        Provider = "OpenAI",
        ApiKey = "server-default-key",
        Model = "gpt-4o-mini",
        BaseUrl = null,
        Temperature = 0.7,
        MaxOutputTokens = 512
    };

    private AiChatServiceFactory CreateSut(AiOptions defaults, Action<HttpRequestMessage> onRequest)
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            onRequest(request);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{ "choices": [ { "index": 0, "message": { "role": "assistant", "content": "hi" } } ] }""",
                    System.Text.Encoding.UTF8, "application/json")
            };
        });

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(handler));
        return new AiChatServiceFactory(httpClientFactory.Object, Options.Create(defaults), NullLoggerFactory.Instance);
    }

    [Fact]
    public async Task Create_SwitchesToAProviderAppropriateDefaultModel_WhenUserOverridesTheProvider()
    {
        // The server default model is an OpenAI model name ("gpt-4o-mini"), which doesn't exist
        // on Groq — a user switching provider via BYOK settings must not inherit it.
        string? capturedModel = null;
        var sut = CreateSut(ServerDefaults(), request =>
        {
            var body = request.Content!.ReadAsStringAsync().Result;
            capturedModel = JsonDocument.Parse(body).RootElement.GetProperty("model").GetString();
        });

        var service = sut.Create("Groq", "user-own-key");
        Assert.Equal("Groq", service.ProviderName);

        await service.GetResponseAsync("hi", [new ChatMessage("user", "hi")], CancellationToken.None);

        Assert.Equal("openai/gpt-oss-20b", capturedModel);
    }

    [Fact]
    public async Task Create_KeepsServerDefaultModel_WhenOverrideProviderMatchesServerDefault()
    {
        // Overriding only the API key (same provider as the server default) should not trigger
        // the "switching provider" model substitution.
        string? capturedModel = null;
        var sut = CreateSut(ServerDefaults(), request =>
        {
            var body = request.Content!.ReadAsStringAsync().Result;
            capturedModel = JsonDocument.Parse(body).RootElement.GetProperty("model").GetString();
        });

        var service = sut.Create("OpenAI", "user-own-key");
        await service.GetResponseAsync("hi", [new ChatMessage("user", "hi")], CancellationToken.None);

        Assert.Equal("gpt-4o-mini", capturedModel);
    }
}
