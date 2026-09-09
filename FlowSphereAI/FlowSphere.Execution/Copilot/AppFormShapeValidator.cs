using System.Text.Json;

namespace FlowSphere.Execution.Copilot;

/// <summary>Structural validation for an AI-generated app: { "name", "description"?, "sections":
/// [ { "title"?, "fields": [ { "key", "type", "label", "required"?, ... } ] } ] }. Catches
/// malformed output before it's transformed into a saved app's FormSchemaJson.</summary>
public static class AppFormShapeValidator
{
    public static readonly HashSet<string> AllowedFieldTypes = new(StringComparer.Ordinal)
    {
        "Text", "TextArea", "Number", "Email", "Phone", "Date", "DateTime", "Checkbox", "Dropdown", "File",
    };

    public static bool TryValidate(string json, out string? error)
    {
        error = null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                error = "The AI's response was not a JSON object.";
                return false;
            }

            if (!root.TryGetProperty("name", out var nameEl) || nameEl.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(nameEl.GetString()))
            {
                error = "The AI's response is missing a non-empty 'name'.";
                return false;
            }

            if (!root.TryGetProperty("sections", out var sectionsEl) || sectionsEl.ValueKind != JsonValueKind.Array || sectionsEl.GetArrayLength() == 0)
            {
                error = "The AI's response has no sections.";
                return false;
            }

            var sawAnyField = false;

            foreach (var section in sectionsEl.EnumerateArray())
            {
                if (section.ValueKind != JsonValueKind.Object || !section.TryGetProperty("fields", out var fieldsEl) || fieldsEl.ValueKind != JsonValueKind.Array)
                {
                    error = "Every section needs a 'fields' array.";
                    return false;
                }

                foreach (var field in fieldsEl.EnumerateArray())
                {
                    if (field.ValueKind != JsonValueKind.Object)
                    {
                        error = "Every field must be a JSON object.";
                        return false;
                    }

                    var key = field.TryGetProperty("key", out var keyEl) && keyEl.ValueKind == JsonValueKind.String ? keyEl.GetString() : null;
                    var type = field.TryGetProperty("type", out var typeEl) && typeEl.ValueKind == JsonValueKind.String ? typeEl.GetString() : null;
                    var label = field.TryGetProperty("label", out var labelEl) && labelEl.ValueKind == JsonValueKind.String ? labelEl.GetString() : null;

                    if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(label))
                    {
                        error = "Every field needs a non-empty 'key' and 'label'.";
                        return false;
                    }

                    if (type is null || !AllowedFieldTypes.Contains(type))
                    {
                        error = $"Field '{key}' has an unsupported type '{type}'.";
                        return false;
                    }

                    sawAnyField = true;
                }
            }

            if (!sawAnyField)
            {
                error = "The AI's response has no fields in any section.";
                return false;
            }

            return true;
        }
        catch (JsonException)
        {
            error = "The AI's response was not valid JSON.";
            return false;
        }
    }
}
