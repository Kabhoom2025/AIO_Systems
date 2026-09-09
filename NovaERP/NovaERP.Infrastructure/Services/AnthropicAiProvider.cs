using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Services;

/// <summary>Calls Anthropic's Messages API (https://docs.anthropic.com/en/api/messages). Always
/// offers get_widget_data; offers propose_create_service_ticket only when
/// AiToolContext.CanCreateServiceTickets is true, so an unauthorized caller's model literally
/// never has the option to call it. Reads the API key/model from the per-organization
/// AiAssistantSettings row at call time (mirrors HttpSmsSender's send-time DB read) —
/// AiAssistantService checks IsConfiguredAsync and never invokes CompleteAsync when no key is
/// set, so a missing key never reaches here as an error path.</summary>
public class AnthropicAiProvider : IAiProvider
{
    private const string ApiVersion = "2023-06-01";

    private const string GetWidgetDataToolJson = """
        {
          "name": "get_widget_data",
          "description": "Fetch a live operational-data summary for the organization. Returns a set of labels and corresponding numeric values (counts or totals).",
          "input_schema": {
            "type": "object",
            "properties": {
              "widgetType": {
                "type": "string",
                "enum": ["SalesOrderStatusSummary", "ServiceTicketStatusSummary", "PosSalesTotal", "ProjectTaskStatusSummary"],
                "description": "SalesOrderStatusSummary: sales orders grouped by status. ServiceTicketStatusSummary: support tickets grouped by status. PosSalesTotal: total revenue from completed point-of-sale sales. ProjectTaskStatusSummary: project tasks grouped by status."
              }
            },
            "required": ["widgetType"]
          }
        }
        """;

    private const string ProposeCreateServiceTicketToolJson = """
        {
          "name": "propose_create_service_ticket",
          "description": "Propose creating a new support ticket for the user to explicitly confirm. This never creates anything by itself — it only shows the user a proposal they must confirm.",
          "input_schema": {
            "type": "object",
            "properties": {
              "subject": { "type": "string", "description": "Short summary of the issue" },
              "description": { "type": "string", "description": "Full details of the issue" },
              "category": { "type": "string", "description": "Ticket category name, e.g. Hardware, Software, or Access — ask the user which if unclear" },
              "priority": { "type": "string", "enum": ["Low", "Medium", "High", "Critical"] }
            },
            "required": ["subject", "description", "category", "priority"]
          }
        }
        """;

    private const string SystemPrompt =
        "You are the NovaERP AI Assistant, helping a user understand their organization's " +
        "day-to-day operational data (sales orders, support tickets, POS sales, project " +
        "tasks). Use the get_widget_data tool when the user asks about any of that data. " +
        "Keep answers concise and grounded in the tool's actual results — do not invent numbers. " +
        "If the propose_create_service_ticket tool is available and the user asks you to log or " +
        "create a support ticket, call it once you have enough detail — it only proposes the " +
        "ticket for the user to confirm, it never creates it directly, so never tell the user " +
        "the ticket has been created until they've confirmed it.";

    private readonly HttpClient _httpClient;
    private readonly NovaErpDbContext _ctx;

    public AnthropicAiProvider(HttpClient httpClient, NovaErpDbContext ctx)
    {
        _httpClient = httpClient;
        _ctx = ctx;
    }

    public async Task<bool> IsConfiguredAsync(int orgId)
    {
        var settings = await _ctx.AiAssistantSettings.FirstOrDefaultAsync(s => s.OrganizationId == orgId);
        return !string.IsNullOrWhiteSpace(settings?.AnthropicApiKey);
    }

    public async Task<AiCompletionResult> CompleteAsync(int orgId, List<AiChatTurn> history, AiToolContext toolContext, CancellationToken ct = default)
    {
        var settings = await _ctx.AiAssistantSettings.FirstOrDefaultAsync(s => s.OrganizationId == orgId, ct);
        var apiKey = settings?.AnthropicApiKey ?? string.Empty;
        var model = settings?.AnthropicModel;
        if (string.IsNullOrWhiteSpace(model)) model = "claude-sonnet-4-5";

        var messages = history.Select(ToAnthropicMessage).ToList();
        var tools = BuildTools(toolContext);

        var requestBody = new
        {
            model,
            max_tokens = 1024,
            system = SystemPrompt,
            messages,
            tools
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/messages")
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", ApiVersion);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = (int)response.StatusCode switch
            {
                401 => "The Anthropic API key is invalid or has been rejected — ask an administrator to check the key in AI Assistant Settings.",
                429 => "The Anthropic API rate limit was hit — please try again in a moment.",
                _ => $"The AI provider returned an error ({(int)response.StatusCode}) — please try again later."
            };
            return new AiCompletionResult { FinalText = errorText };
        }

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseBody);
        var content = doc.RootElement.GetProperty("content");

        string? finalText = null;
        string? toolCallName = null;
        string? toolCallArgumentsJson = null;
        string? toolCallId = null;

        foreach (var block in content.EnumerateArray())
        {
            var type = block.GetProperty("type").GetString();
            if (type == "tool_use")
            {
                toolCallId = block.GetProperty("id").GetString();
                toolCallName = block.GetProperty("name").GetString();
                toolCallArgumentsJson = block.GetProperty("input").GetRawText();
            }
            else if (type == "text")
            {
                finalText = (finalText ?? string.Empty) + block.GetProperty("text").GetString();
            }
        }

        return new AiCompletionResult
        {
            FinalText = toolCallName is null ? finalText : null,
            ToolCallName = toolCallName,
            ToolCallArgumentsJson = toolCallArgumentsJson,
            ToolCallId = toolCallId
        };
    }

    private static List<object> BuildTools(AiToolContext toolContext)
    {
        var tools = new List<object> { JsonSerializer.Deserialize<object>(GetWidgetDataToolJson)! };
        if (toolContext.CanCreateServiceTickets)
            tools.Add(JsonSerializer.Deserialize<object>(ProposeCreateServiceTicketToolJson)!);
        return tools;
    }

    private static object ToAnthropicMessage(AiChatTurn turn)
    {
        if (turn.Role == "assistant_tool_use")
        {
            return new
            {
                role = "assistant",
                content = new object[]
                {
                    new
                    {
                        type = "tool_use",
                        id = turn.ToolCallId,
                        name = turn.ToolName ?? "get_widget_data",
                        input = JsonSerializer.Deserialize<object>(turn.Content)
                    }
                }
            };
        }

        if (turn.Role == "tool")
        {
            return new
            {
                role = "user",
                content = new object[]
                {
                    new
                    {
                        type = "tool_result",
                        tool_use_id = turn.ToolCallId,
                        content = turn.Content
                    }
                }
            };
        }

        return new { role = turn.Role, content = turn.Content };
    }
}
