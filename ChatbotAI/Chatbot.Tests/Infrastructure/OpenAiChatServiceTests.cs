using System.Net;
using Chatbot.Application.Common.Exceptions;
using Chatbot.Application.Common.Interfaces;
using Chatbot.Infrastructure.Ai;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Chatbot.Tests.Infrastructure;

public class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(responder(request));
}

public class OpenAiChatServiceTests
{
    private static AiOptions DefaultOptions() => new()
    {
        Provider = "OpenAI",
        ApiKey = "test-key",
        Model = "gpt-4o-mini",
        Temperature = 0.7,
        MaxOutputTokens = 512
    };

    [Fact]
    public async Task GetResponseAsync_ParsesContentAndUsage_FromSuccessfulResponse()
    {
        const string json = """
        {
          "id": "chatcmpl-1",
          "model": "gpt-4o-mini",
          "choices": [ { "index": 0, "message": { "role": "assistant", "content": "Hello there!" }, "finish_reason": "stop" } ],
          "usage": { "prompt_tokens": 12, "completion_tokens": 4 }
        }
        """;

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        });
        var httpClient = new HttpClient(handler);
        var sut = new OpenAiChatService(httpClient, Options.Create(DefaultOptions()), NullLogger<OpenAiChatService>.Instance);

        var result = await sut.GetResponseAsync("Hi", [new ChatMessage("user", "Hi")], CancellationToken.None);

        Assert.Equal("Hello there!", result.Content);
        Assert.Equal(12, result.PromptTokens);
        Assert.Equal(4, result.CompletionTokens);
        Assert.Equal("gpt-4o-mini", result.Model);
    }

    [Fact]
    public async Task GetResponseAsync_ThrowsAiServiceException_WhenProviderReturnsError()
    {
        const string json = """{ "error": { "message": "invalid_api_key", "type": "invalid_request_error" } }""";

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        });
        var httpClient = new HttpClient(handler);
        var sut = new OpenAiChatService(httpClient, Options.Create(DefaultOptions()), NullLogger<OpenAiChatService>.Instance);

        var ex = await Assert.ThrowsAsync<AiServiceException>(() =>
            sut.GetResponseAsync("Hi", [new ChatMessage("user", "Hi")], CancellationToken.None));

        Assert.Equal("invalid_api_key", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetResponseAsync_FallsBackToDefaultEndpoint_WhenBaseUrlIsBlank(string? blankBaseUrl)
    {
        // A blank (as opposed to null) AI:BaseUrl is exactly what a config binder produces for
        // an empty "BaseUrl": "" in appsettings.json — it must not be treated as "explicitly
        // override the endpoint with nothing", which previously produced a relative request URI
        // ("/chat/completions") that HttpClient rejected outright.
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
        var options = DefaultOptions();
        options.BaseUrl = blankBaseUrl;
        var httpClient = new HttpClient(handler);
        var sut = new OpenAiChatService(httpClient, Options.Create(options), NullLogger<OpenAiChatService>.Instance);

        await sut.GetResponseAsync("Hi", [new ChatMessage("user", "Hi")], CancellationToken.None);

        Assert.NotNull(capturedUri);
        Assert.True(capturedUri!.IsAbsoluteUri);
        Assert.Equal("https://api.openai.com/v1/chat/completions", capturedUri.ToString());
    }

    [Fact]
    public async Task StreamResponseAsync_YieldsIncrementalDeltasThenAFinalChunk()
    {
        const string sse =
            "data: {\"model\":\"gpt-4o-mini\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Hel\"}}]}\n\n" +
            "data: {\"model\":\"gpt-4o-mini\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"lo\"}}]}\n\n" +
            "data: {\"model\":\"gpt-4o-mini\",\"usage\":{\"prompt_tokens\":5,\"completion_tokens\":2},\"choices\":[{\"index\":0,\"delta\":{},\"finish_reason\":\"stop\"}]}\n\n" +
            "data: [DONE]\n\n";

        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(sse, System.Text.Encoding.UTF8, "text/event-stream")
        });
        var httpClient = new HttpClient(handler);
        var sut = new OpenAiChatService(httpClient, Options.Create(DefaultOptions()), NullLogger<OpenAiChatService>.Instance);

        var chunks = new List<AiResponseChunk>();
        await foreach (var chunk in sut.StreamResponseAsync("Hi", [new ChatMessage("user", "Hi")], CancellationToken.None))
        {
            chunks.Add(chunk);
        }

        Assert.Equal("Hel", chunks[0].DeltaContent);
        Assert.Equal("lo", chunks[1].DeltaContent);
        Assert.True(chunks[^1].IsFinal);
        Assert.Equal(5, chunks[^1].PromptTokens);
        Assert.Equal(2, chunks[^1].CompletionTokens);
    }
}
