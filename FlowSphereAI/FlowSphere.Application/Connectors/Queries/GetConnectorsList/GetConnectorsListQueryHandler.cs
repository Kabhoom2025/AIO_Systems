using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Connectors.Queries.GetConnectorsList;

public class GetConnectorsListQueryHandler : IRequestHandler<GetConnectorsListQuery, Result<List<ConnectorSummaryDto>>>
{
    private static readonly Dictionary<string, string[]> RequiredCredentialKeysByType = new()
    {
        ["Http"] = Array.Empty<string>(),
        ["Smtp"] = new[] { "Host", "Port", "Username", "Password" },
        ["OpenAi"] = new[] { "ApiKey" },
        ["Slack"] = new[] { "WebhookUrl" },
        ["Teams"] = new[] { "WebhookUrl" },
        ["Discord"] = new[] { "WebhookUrl" },
        ["Webhook"] = new[] { "WebhookUrl" },
        ["Twilio"] = new[] { "AccountSid", "AuthToken", "FromNumber" },
        ["Notion"] = new[] { "ApiKey" },
        ["Jira"] = new[] { "BaseUrl", "Email", "ApiToken" },
        ["Airtable"] = new[] { "ApiKey", "BaseId" },
        ["GoogleSheets"] = new[] { "ServiceAccountJson" },
    };

    // BaseUrl/Model let the same "OpenAi" connector talk to any OpenAI-compatible provider -
    // e.g. set BaseUrl to https://api.x.ai/v1/chat/completions and Model to a Grok model name
    // to use an xAI/Grok key instead of an OpenAI one.
    private static readonly Dictionary<string, string[]> OptionalCredentialKeysByType = new()
    {
        ["Http"] = Array.Empty<string>(),
        ["Smtp"] = Array.Empty<string>(),
        ["OpenAi"] = new[] { "BaseUrl", "Model" },
        ["Slack"] = Array.Empty<string>(),
        ["Teams"] = Array.Empty<string>(),
        ["Discord"] = Array.Empty<string>(),
        ["Webhook"] = new[] { "AuthHeaderValue" },
        ["Twilio"] = Array.Empty<string>(),
        ["Notion"] = Array.Empty<string>(),
        ["Jira"] = Array.Empty<string>(),
        ["Airtable"] = Array.Empty<string>(),
        ["GoogleSheets"] = Array.Empty<string>(),
    };

    private readonly IApplicationDbContext _db;

    public GetConnectorsListQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<ConnectorSummaryDto>>> Handle(GetConnectorsListQuery request, CancellationToken cancellationToken)
    {
        var connectors = await _db.Connectors
            .Where(c => c.OrganizationId == null)
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Type, c.Name, c.IsEnabled })
            .ToListAsync(cancellationToken);

        var result = connectors
            .Select(c => new ConnectorSummaryDto(
                c.Id, c.Type, c.Name, c.IsEnabled,
                RequiredCredentialKeysByType.GetValueOrDefault(c.Type, Array.Empty<string>()),
                OptionalCredentialKeysByType.GetValueOrDefault(c.Type, Array.Empty<string>())))
            .ToList();

        return Result<List<ConnectorSummaryDto>>.Success(result);
    }
}
