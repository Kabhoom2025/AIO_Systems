using System.Text.Json;
using System.Text.RegularExpressions;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Execution.Connectors;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Execution.Copilot;

/// <summary>Generates a ready-to-save app (name + form schema) from a natural-language prompt by
/// composing a schema-describing system prompt with the user's request and delegating to the
/// OpenAi connector - the same connector/credential path WorkflowGraphGenerator uses.</summary>
public class AppFormGenerator : IAppFormGenerator
{
    private const string SchemaInstructions = """
        You are a form generator for a no-code app builder called FlowSphere AI. Given a user's
        plain-English description of a form/app, respond with ONLY a single valid JSON object -
        no markdown formatting, no code fences, no explanation before or after it. The JSON must
        match this exact shape:

        {
          "name": "<short app name, title case, max 60 chars>",
          "description": "<one-sentence description, or null>",
          "sections": [
            {
              "title": "<short section heading, e.g. 'Details'>",
              "fields": [
                {
                  "key": "<unique short lowercase_snake_case identifier>",
                  "type": "<one of: Text, TextArea, Number, Email, Phone, Date, DateTime, Checkbox, Dropdown, File>",
                  "label": "<human-readable field label>",
                  "required": <true|false>,
                  "placeholder": "<optional short hint text, or omit>",
                  "helpText": "<optional short helper text shown below the field, or omit>",
                  "options": ["<only for type Dropdown - 2-8 sensible options>"]
                }
              ]
            }
          ]
        }

        Rules:
        - Group related fields into 1-4 sections with clear titles. A short form can be one section.
        - Use "Dropdown" (with an "options" array) for closed-choice fields, not free-text "Text".
        - Use "Email"/"Phone"/"Date"/"DateTime"/"Number" instead of generic "Text" whenever the
          field's real-world content matches one of those types.
        - Keep it to at most 12 fields total unless the request clearly needs more.
        - Field keys must be unique across the whole form, not just within a section.
        """;

    private readonly ConnectorRegistry _connectorRegistry;
    private readonly IConnectorCredentialStore _credentialStore;
    private readonly IApplicationDbContext _db;

    public AppFormGenerator(ConnectorRegistry connectorRegistry, IConnectorCredentialStore credentialStore, IApplicationDbContext db)
    {
        _connectorRegistry = connectorRegistry;
        _credentialStore = credentialStore;
        _db = db;
    }

    public async Task<Result<GeneratedAppFormResult>> GenerateAsync(string prompt, int organizationId, CancellationToken cancellationToken)
    {
        var openAiConnectorId = await _db.Connectors
            .Where(c => c.Type == "OpenAi" && c.OrganizationId == null)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (openAiConnectorId == 0)
        {
            return Result<GeneratedAppFormResult>.Failure(Error.Unexpected("No OpenAI connector is configured on this platform."));
        }

        var apiKey = await _credentialStore.GetAsync(openAiConnectorId, "ApiKey", organizationId, cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Result<GeneratedAppFormResult>.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["apiKey"] = new[] { "No OpenAI API key is configured for your organization yet. Add one under Connectors before using AI app generation." },
            }));
        }

        var baseUrl = await _credentialStore.GetAsync(openAiConnectorId, "BaseUrl", organizationId, cancellationToken);
        var model = await _credentialStore.GetAsync(openAiConnectorId, "Model", organizationId, cancellationToken);

        var connectorConfig = JsonSerializer.Serialize(new
        {
            apiKey,
            model = string.IsNullOrWhiteSpace(model) ? "gpt-4o-mini" : model,
            prompt = $"{SchemaInstructions}\n\nUser's request: {prompt}",
            baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl,
        });

        var connector = _connectorRegistry.Resolve("OpenAi");
        var invocationResult = await connector.InvokeAsync(new ConnectorInvocation(connectorConfig, "{}"), cancellationToken);

        if (!invocationResult.Success)
        {
            return Result<GeneratedAppFormResult>.Failure(Error.Unexpected($"AI generation failed: {invocationResult.ErrorMessage}"));
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
            return Result<GeneratedAppFormResult>.Failure(Error.Unexpected("The AI provider returned an unexpected response shape."));
        }

        var appJson = StripCodeFences(completion);

        if (!AppFormShapeValidator.TryValidate(appJson, out var validationError))
        {
            return Result<GeneratedAppFormResult>.Failure(Error.Unexpected(
                $"The AI returned an unusable app: {validationError} Try rephrasing your request."));
        }

        var result = BuildResult(appJson);
        return Result<GeneratedAppFormResult>.Success(result);
    }

    /// <summary>Transforms the AI's validated-but-raw JSON into the final app: sanitizes/dedupes
    /// field keys, assigns each field a grid position (alternating half-width, two per row) since
    /// asking the model to reason about 12-column grid math itself is unreliable, and reshapes
    /// into the exact { "sections": [ { "id", "title", "fields": [...] } ] } the client expects.</summary>
    private static GeneratedAppFormResult BuildResult(string appJson)
    {
        using var doc = JsonDocument.Parse(appJson);
        var root = doc.RootElement;

        var name = root.GetProperty("name").GetString()!.Trim();
        var description = root.TryGetProperty("description", out var descEl) && descEl.ValueKind == JsonValueKind.String
            ? descEl.GetString()
            : null;

        var usedKeys = new HashSet<string>(StringComparer.Ordinal);
        var sections = new List<object>();
        var sectionIndex = 0;

        foreach (var section in root.GetProperty("sections").EnumerateArray())
        {
            sectionIndex++;
            var title = section.TryGetProperty("title", out var titleEl) && titleEl.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(titleEl.GetString())
                ? titleEl.GetString()!
                : $"Section {sectionIndex}";

            var fields = new List<object>();
            var fieldIndex = 0;

            foreach (var field in section.GetProperty("fields").EnumerateArray())
            {
                var type = field.GetProperty("type").GetString()!;
                var label = field.GetProperty("label").GetString()!;
                var rawKey = field.GetProperty("key").GetString()!;
                var key = MakeUniqueKey(rawKey, usedKeys);

                var required = field.TryGetProperty("required", out var reqEl) && reqEl.ValueKind == JsonValueKind.True;
                var placeholder = field.TryGetProperty("placeholder", out var phEl) && phEl.ValueKind == JsonValueKind.String ? phEl.GetString() : null;
                var helpText = field.TryGetProperty("helpText", out var htEl) && htEl.ValueKind == JsonValueKind.String ? htEl.GetString() : null;
                List<string>? options = null;
                if (type == "Dropdown" && field.TryGetProperty("options", out var optEl) && optEl.ValueKind == JsonValueKind.Array)
                {
                    options = optEl.EnumerateArray()
                        .Where(o => o.ValueKind == JsonValueKind.String)
                        .Select(o => o.GetString()!)
                        .ToList();
                }
                if (type == "Dropdown" && (options is null || options.Count == 0))
                {
                    options = new List<string> { "Option 1" };
                }

                var w = 6;
                var x = (fieldIndex % 2) * w;
                var y = fieldIndex / 2;
                fieldIndex++;

                fields.Add(new
                {
                    key,
                    type,
                    label,
                    required,
                    placeholder,
                    helpText,
                    options,
                    layout = new { x, y, w, h = 1 },
                });
            }

            sections.Add(new { id = $"section_{sectionIndex}", title, fields });
        }

        var formSchemaJson = JsonSerializer.Serialize(new { sections });
        return new GeneratedAppFormResult(name, description, formSchemaJson);
    }

    private static string MakeUniqueKey(string rawKey, HashSet<string> usedKeys)
    {
        var slug = Regex.Replace(rawKey.Trim().ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
        if (string.IsNullOrEmpty(slug))
        {
            slug = "field";
        }

        var candidate = slug;
        var suffix = 2;
        while (!usedKeys.Add(candidate))
        {
            candidate = $"{slug}_{suffix}";
            suffix++;
        }

        return candidate;
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
