using Chatbot.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chatbot.Infrastructure.Ai;

/// <summary>Talks to an Azure OpenAI resource deployment.</summary>
public class AzureOpenAiChatService(HttpClient httpClient, IOptions<AiOptions> options, ILogger<AzureOpenAiChatService> logger)
    : OpenAiCompatibleChatServiceBase(httpClient, options, logger)
{
    public override string ProviderName => "AzureOpenAI";

    protected override string BuildRequestUri()
    {
        if (string.IsNullOrWhiteSpace(Options.AzureEndpoint))
        {
            throw new AiServiceException("AI:AzureEndpoint must be configured when AI:Provider is 'AzureOpenAI'.");
        }
        if (string.IsNullOrWhiteSpace(Options.AzureDeploymentName))
        {
            throw new AiServiceException("AI:AzureDeploymentName must be configured when AI:Provider is 'AzureOpenAI'.");
        }

        var endpoint = Options.AzureEndpoint.TrimEnd('/');
        return $"{endpoint}/openai/deployments/{Options.AzureDeploymentName}/chat/completions?api-version={Options.AzureApiVersion}";
    }

    protected override void ApplyAuthHeaders(HttpRequestMessage request) =>
        request.Headers.Add("api-key", Options.ApiKey);
}
