using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Execution.Connectors;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Execution.Copilot;

/// <summary>Generates a WorkflowGraph-shaped JSON string from a natural-language prompt by
/// composing a schema-describing system prompt with the user's request and delegating to the
/// OpenAi connector - the same connector/credential path AiPromptNodeExecutor uses, so this
/// requires the org to have already configured an OpenAI API key under Connectors.</summary>
public class WorkflowGraphGenerator : IWorkflowGraphGenerator
{
    private const string SchemaInstructions = """
        You are a workflow graph generator for an automation platform called FlowSphere AI.
        Given a user's plain-English description of an automation, respond with ONLY a single
        valid JSON object - no markdown formatting, no code fences, no explanation before or
        after it. The JSON must match this exact shape:

        {
          "nodes": [ { "key": "<unique short lowercase id>", "type": "<node type>", "config": { ... } } ],
          "edges": [ { "source": "<node key>", "target": "<node key>", "sourceHandle": "true" | "false" | null } ]
        }

        Allowed node types and their config shape:
        - "Trigger": config must be {} (empty object). Exactly one Trigger node must exist, with
          no incoming edges - it is always the starting point.
        - "HttpRequest": config = { "method": "GET"|"POST"|"PUT"|"DELETE", "url": "<string>",
          "body": "<optional raw string>" }.
        - "Condition": config = { "field": "<dot-path into the upstream JSON, e.g. body.statusCode>",
          "operator": "equals"|"notEquals"|"contains"|"greaterThan"|"lessThan", "value": "<string>" }.
          A Condition node MUST have exactly two outgoing edges: one with "sourceHandle": "true"
          and one with "sourceHandle": "false".
        - "Delay": config = { "milliseconds": <number, max 60000> }.
        - "EmailSmtp": config = { "to": "<email>", "subject": "<string>", "body": "<string>" }.
        - "AiPrompt": config = { "model": "gpt-4o-mini", "prompt": "<string>" }.
        - "SlackMessage": config = { "text": "<string>", "channel": "<optional #channel-name override>" }.
        - "TeamsMessage": config = { "text": "<string>", "title": "<optional bolded card heading>" }.
        - "DiscordMessage": config = { "content": "<string>" }.
        - "Decision": config = { "field": "<dot-path>", "operator": "equals"|"notEquals"|"contains"|
          "greaterThan"|"lessThan", "cases": [ { "value": "<string>", "handle": "<short id>" } ],
          "defaultHandle": "<short id>" }. Must have one outgoing edge per case's "handle" plus one
          for "defaultHandle", each edge's sourceHandle matching exactly.
        - "Exception": config = {}. Only reachable via an edge with "sourceHandle": "error" coming
          out of the node whose failure it should handle - do not generate this unless the user
          explicitly asks for error handling.
        - "Loop": config = { "arrayPath": "<dot-path into the upstream JSON pointing at an array>" }.
          Must have exactly ONE outgoing edge, to the single node that runs once per array item
          (its "body"). Do not chain multiple nodes as the body - only the one directly connected
          to the Loop node runs per iteration; whatever it connects to next runs once, after the
          loop finishes, receiving an array of the body's per-iteration outputs.
        - "Parallel": config = { "mergeNodeKey": "<key of a Merge node also in this graph>" }.
          Must have at least two outgoing edges (the branches, no sourceHandle needed - they all
          run concurrently) and its mergeNodeKey MUST exactly match the "key" of a "Merge" node
          you also create in the same graph, which every branch eventually connects into.
        - "Merge": config = {}. Exists only as the join point a Parallel node's mergeNodeKey
          points to - every one of that Parallel's branches must have an edge leading to it.
        - "UserTask": config = { "assigneeLabel": "<optional short label of who reviews this
          step, e.g. 'HR', 'Finance Team', 'CFO' - purely descriptive, shown to whoever resolves
          it, not an access-control assignment>" }. Suspends the workflow for a human decision.
          Must have exactly two outgoing edges: one with "sourceHandle": "approved" and one with
          "sourceHandle": "rejected". Only generate this when the user explicitly asks for an
          approval step. For a multi-level approval chain (e.g. "manager approves, then finance
          reviews if the amount is over $X, then CFO signs off above $Y"), chain a Condition node
          (on the amount/field) into separate UserTask nodes with distinct assigneeLabels per
          level - each UserTask's "approved" edge leads either to the next approval level's
          Condition/UserTask, or to a terminal action once the chain is satisfied.
        - "Webhook": config = { "payload": "<raw JSON string to POST>" }. Posts to the org's
          configured custom webhook URL - use this for "send this to my own endpoint" requests
          that aren't one of the other specific integrations below.
        - "Twilio": config = { "to": "<E.164 phone number, e.g. +15551234567>", "body": "<SMS text>" }.
        - "Notion": config = { "databaseId": "<target Notion database id>", "title": "<page title>" }.
        - "Jira": config = { "projectKey": "<project key, e.g. OPS>", "summary": "<issue summary>",
          "description": "<optional issue description>", "issueType": "<optional, defaults to Task>" }.
        - "Airtable": config = { "table": "<table name>", "fieldsJson": "<raw JSON object string
          of field name/value pairs, e.g. {\"Name\":\"Alice\",\"Status\":\"Active\"}>" }.
        - "GoogleSheets": config = { "spreadsheetId": "<target spreadsheet id>", "sheetName":
          "<optional, defaults to Sheet1>", "rowValuesJson": "<raw JSON array string of cell
          values for one row, e.g. [\"Alice\",\"30\",\"Active\"]>" }.

        Worked example combining Parallel + Merge (notice branchA and branchB both point to
        "join", which is the node parallel1.config.mergeNodeKey names):
        {
          "nodes": [
            { "key": "trigger1", "type": "Trigger", "config": {} },
            { "key": "parallel1", "type": "Parallel", "config": { "mergeNodeKey": "join" } },
            { "key": "branchA", "type": "SlackMessage", "config": { "text": "Branch A" } },
            { "key": "branchB", "type": "TeamsMessage", "config": { "text": "Branch B" } },
            { "key": "join", "type": "Merge", "config": {} }
          ],
          "edges": [
            { "source": "trigger1", "target": "parallel1" },
            { "source": "parallel1", "target": "branchA" },
            { "source": "parallel1", "target": "branchB" },
            { "source": "branchA", "target": "join" },
            { "source": "branchB", "target": "join" }
          ]
        }

        Rules:
        - Every node except Trigger must have exactly one incoming edge, except nodes downstream
          of a Condition's two branches.
        - Do not create cycles.
        - Keep the workflow to at most 6 nodes unless the request clearly needs more.
        - Node keys must be unique.
        """;

    private readonly ConnectorRegistry _connectorRegistry;
    private readonly IConnectorCredentialStore _credentialStore;
    private readonly IApplicationDbContext _db;

    public WorkflowGraphGenerator(ConnectorRegistry connectorRegistry, IConnectorCredentialStore credentialStore, IApplicationDbContext db)
    {
        _connectorRegistry = connectorRegistry;
        _credentialStore = credentialStore;
        _db = db;
    }

    public async Task<Result<string>> GenerateGraphJsonAsync(
        string prompt, string? currentGraphJson, int organizationId, CancellationToken cancellationToken)
    {
        var openAiConnectorId = await _db.Connectors
            .Where(c => c.Type == "OpenAi" && c.OrganizationId == null)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (openAiConnectorId == 0)
        {
            return Result<string>.Failure(Error.Unexpected("No OpenAI connector is configured on this platform."));
        }

        var apiKey = await _credentialStore.GetAsync(openAiConnectorId, "ApiKey", organizationId, cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Result<string>.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["apiKey"] = new[] { "No OpenAI API key is configured for your organization yet. Add one under Connectors before using the AI workflow generator." }
            }));
        }

        // BaseUrl/Model are optional overrides so this same connector works with any
        // OpenAI-compatible provider (e.g. xAI/Grok) without a separate connector type.
        var baseUrl = await _credentialStore.GetAsync(openAiConnectorId, "BaseUrl", organizationId, cancellationToken);
        var model = await _credentialStore.GetAsync(openAiConnectorId, "Model", organizationId, cancellationToken);

        var combinedPrompt = string.IsNullOrWhiteSpace(currentGraphJson)
            ? $"{SchemaInstructions}\n\nUser's request: {prompt}"
            : $"""
               {SchemaInstructions}

               The user already has this workflow graph on their canvas:
               {currentGraphJson}

               Modify that graph according to the following instruction and return the FULL
               updated graph (not a diff, not just the changed nodes) in the same JSON shape.
               Keep existing node keys stable for nodes you don't need to change so unrelated
               parts of the workflow aren't needlessly renamed.

               Instruction: {prompt}
               """;
        var connectorConfig = JsonSerializer.Serialize(new
        {
            apiKey,
            model = string.IsNullOrWhiteSpace(model) ? "gpt-4o-mini" : model,
            prompt = combinedPrompt,
            baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl,
        });

        var connector = _connectorRegistry.Resolve("OpenAi");
        var invocationResult = await connector.InvokeAsync(new ConnectorInvocation(connectorConfig, "{}"), cancellationToken);

        if (!invocationResult.Success)
        {
            return Result<string>.Failure(Error.Unexpected($"AI generation failed: {invocationResult.ErrorMessage}"));
        }

        string completion;
        try
        {
            using var outputDoc = JsonDocument.Parse(invocationResult.OutputJson ?? "{}");
            completion = outputDoc.RootElement.TryGetProperty("completion", out var completionEl)
                ? completionEl.GetString() ?? ""
                : "";
        }
        catch (JsonException)
        {
            return Result<string>.Failure(Error.Unexpected("The AI provider returned an unexpected response shape."));
        }

        var graphJson = StripCodeFences(completion);

        if (!GraphShapeValidator.TryValidate(graphJson, out var validationError))
        {
            return Result<string>.Failure(Error.Unexpected(
                $"The AI returned an unusable workflow graph: {validationError} Try rephrasing your request."));
        }

        return Result<string>.Success(graphJson);
    }

    private static string StripCodeFences(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (firstNewline >= 0 && lastFence > firstNewline)
            {
                trimmed = trimmed[(firstNewline + 1)..lastFence].Trim();
            }
        }

        return trimmed;
    }
}
