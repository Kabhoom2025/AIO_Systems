using System.Text.Json;
using System.Text.Json.Nodes;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Infrastructure.Notifications;

public class AppNotificationDispatcher : IAppNotificationDispatcher
{
    private readonly IApplicationDbContext _db;
    private readonly IAppNotificationEmailSender _emailSender;

    public AppNotificationDispatcher(IApplicationDbContext db, IAppNotificationEmailSender emailSender)
    {
        _db = db;
        _emailSender = emailSender;
    }

    public async Task DispatchRecordSubmittedAsync(int organizationId, int appId, string dataJson, CancellationToken cancellationToken)
    {
        var rules = await _db.AppNotificationRules
            .Where(r => r.AppId == appId && r.IsEnabled && r.Channel == AppNotificationChannel.Email
                && (r.Event == AppNotificationEvent.RecordSubmitted || r.Event == AppNotificationEvent.Custom))
            .ToListAsync(cancellationToken);

        if (rules.Count == 0)
        {
            return;
        }

        var data = JsonNode.Parse(dataJson)?.AsObject() ?? new JsonObject();

        foreach (var rule in rules)
        {
            if (rule.Event == AppNotificationEvent.Custom && !ConditionsMatch(rule.ConditionsJson, data))
            {
                continue;
            }

            var to = await ResolveRecipientsAsync(organizationId, rule, data, cancellationToken);
            if (to.Count == 0)
            {
                continue;
            }

            var subject = SubstitutePlaceholders(rule.Subject, data);
            var body = SubstitutePlaceholders(rule.Body, data);

            await _emailSender.SendAsync(organizationId, to, Array.Empty<string>(), subject, body, cancellationToken);
        }
    }

    private async Task<List<string>> ResolveRecipientsAsync(int organizationId, Domain.Entities.AppNotificationRule rule, JsonObject data, CancellationToken cancellationToken)
    {
        switch (rule.RecipientType)
        {
            case AppNotificationRecipientType.DataField:
                if (string.IsNullOrEmpty(rule.RecipientDataFieldKey))
                {
                    return new List<string>();
                }
                var email = data.TryGetPropertyValue(rule.RecipientDataFieldKey, out var value) ? value?.GetValue<string>() : null;
                return string.IsNullOrWhiteSpace(email) ? new List<string>() : new List<string> { email };

            case AppNotificationRecipientType.Users:
                var userIds = ParseIntArray(rule.RecipientUserIdsJson);
                return await _db.Users
                    .Where(u => u.OrganizationId == organizationId && userIds.Contains(u.Id))
                    .Select(u => u.Email)
                    .ToListAsync(cancellationToken);

            case AppNotificationRecipientType.Roles:
                var roleIds = ParseIntArray(rule.RecipientRoleIdsJson);
                return await _db.Users
                    .Where(u => u.OrganizationId == organizationId && u.RoleId != null && roleIds.Contains(u.RoleId.Value))
                    .Select(u => u.Email)
                    .ToListAsync(cancellationToken);

            default:
                return new List<string>();
        }
    }

    private static List<int> ParseIntArray(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Array
                ? doc.RootElement.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.Number).Select(e => e.GetInt32()).ToList()
                : new List<int>();
        }
        catch (JsonException)
        {
            return new List<int>();
        }
    }

    /// <summary>List of { fieldKey, operator, value } - the same shape the client's Conditions
    /// builder writes. Numeric comparisons parse both sides as double; anything else falls back to
    /// ordinal string comparison.</summary>
    private static bool ConditionsMatch(string conditionsJson, JsonObject data)
    {
        try
        {
            using var doc = JsonDocument.Parse(conditionsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
            {
                return true;
            }

            foreach (var condition in doc.RootElement.EnumerateArray())
            {
                var fieldKey = condition.GetProperty("fieldKey").GetString() ?? "";
                var op = condition.GetProperty("operator").GetString() ?? "equals";
                var expected = condition.TryGetProperty("value", out var v) ? v.ToString() : "";

                var actual = data.TryGetPropertyValue(fieldKey, out var actualNode) ? actualNode?.ToString() ?? "" : "";

                if (!Compare(actual, op, expected))
                {
                    return false;
                }
            }

            return true;
        }
        catch (JsonException)
        {
            return true;
        }
    }

    private static bool Compare(string actual, string op, string expected)
    {
        if (double.TryParse(actual, out var a) && double.TryParse(expected, out var e))
        {
            return op switch
            {
                "greaterThan" => a > e,
                "lessThan" => a < e,
                "greaterOrEqual" => a >= e,
                "lessOrEqual" => a <= e,
                "notEquals" => a != e,
                _ => a == e,
            };
        }

        return op switch
        {
            "notEquals" => !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            "contains" => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            _ => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
        };
    }

    private static string SubstitutePlaceholders(string template, JsonObject data)
    {
        var result = template;
        foreach (var (key, value) in data)
        {
            result = result.Replace($"@{key}", value?.ToString() ?? "", StringComparison.Ordinal);
        }

        return result;
    }
}
