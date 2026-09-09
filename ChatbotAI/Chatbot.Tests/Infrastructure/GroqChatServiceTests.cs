using System.Net;
using Chatbot.Application.Common.Interfaces;
using Chatbot.Infrastructure.Ai;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Chatbot.Tests.Infrastructure;

public class GroqChatServiceTests
{
    [Fact]
    public async Task GetResponseAsync_TargetsGroqsOwnEndpoint_NotXaisGrokEndpoint()
    {
        Uri? capturedUri = null;
        var handler = new FakeHttpMessageHandler(request =>
        {
            capturedUri = request.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{ "choices": [ { "index": 0, "message": { "role": "assistant", "content": "hi" } } ] }""",
                    System.Text.Encoding.UTF8, "application/json")
            };
        });
        var options = new AiOptions { Provider = "Groq", ApiKey = "gsk_test", Model = "llama-3.3-70b-versatile" };
        var sut = new GroqChatService(new HttpClient(handler), Options.Create(options), NullLogger<GroqChatService>.Instance);

        await sut.GetResponseAsync("Hi", [new ChatMessage("user", "Hi")], CancellationToken.None);

        Assert.Equal("Groq", sut.ProviderName);
        Assert.Equal("https://api.groq.com/openai/v1/chat/completions", capturedUri!.ToString());
    }
}
