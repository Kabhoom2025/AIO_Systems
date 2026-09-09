using System.Text.Json;
using System.Text.Json.Nodes;

namespace Platform.Lineage.Recording;

/// <summary>
/// Masks sensitive fields before event metadata is persisted to platform.lineage_events, per the
/// spec's "mask passwords, tokens, secrets and configurable sensitive fields". Field names are
/// matched case-insensitively against a configurable list, applied recursively through nested
/// objects/arrays, so no caller needs to remember to mask anything itself.
/// </summary>
public class MetadataMasker
{
    private const string MaskedValue = "***MASKED***";

    private static readonly string[] DefaultSensitiveFieldNames =
    [
        "password", "passwd", "pwd", "secret", "token", "accesstoken", "refreshtoken",
        "apikey", "api_key", "authorization", "creditcard", "cardnumber", "cvv", "ssn",
    ];

    private readonly HashSet<string> _sensitiveFieldNames;

    public MetadataMasker(IEnumerable<string>? additionalSensitiveFieldNames = null)
    {
        _sensitiveFieldNames = new HashSet<string>(DefaultSensitiveFieldNames, StringComparer.OrdinalIgnoreCase);
        if (additionalSensitiveFieldNames is not null)
            _sensitiveFieldNames.UnionWith(additionalSensitiveFieldNames);
    }

    public string? MaskToJson(IReadOnlyDictionary<string, object?>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
            return null;

        var node = new JsonObject();
        foreach (var (key, value) in metadata)
            node[key] = MaskValue(key, value);

        return node.ToJsonString();
    }

    private JsonNode? MaskValue(string key, object? value)
    {
        if (_sensitiveFieldNames.Contains(key))
            return JsonValue.Create(MaskedValue);

        return value switch
        {
            null => null,
            IReadOnlyDictionary<string, object?> nested => MaskNested(nested),
            IEnumerable<object?> items when value is not string => MaskArray(items),
            string s => JsonValue.Create(s),
            bool b => JsonValue.Create(b),
            int i => JsonValue.Create(i),
            long l => JsonValue.Create(l),
            double d => JsonValue.Create(d),
            decimal m => JsonValue.Create(m),
            _ => JsonValue.Create(value.ToString()),
        };
    }

    private JsonObject MaskNested(IReadOnlyDictionary<string, object?> nested)
    {
        var node = new JsonObject();
        foreach (var (key, value) in nested)
            node[key] = MaskValue(key, value);
        return node;
    }

    private JsonArray MaskArray(IEnumerable<object?> items)
    {
        var array = new JsonArray();
        foreach (var item in items)
            array.Add(MaskValue(string.Empty, item));
        return array;
    }
}
