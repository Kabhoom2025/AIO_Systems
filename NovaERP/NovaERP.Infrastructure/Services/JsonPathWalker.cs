using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NovaERP.Infrastructure.Services;

/// <summary>Simple dot/bracket JSON path get/set — deliberately not a full JSONPath
/// implementation. SetValue is always called with a concrete numeric index already resolved
/// (e.g. "packages[0].weight", built by replacing "[]" with a known package index) since the
/// caller always knows how many packages exist. GetValues accepts a single unresolved "[]"
/// wildcard (e.g. "rates[].price") and returns one value per element of that array, since the
/// caller doesn't know the response array's length ahead of time. One wildcard per path is
/// enough for the packages/rates use case this connector engine targets (see
/// ShippingConnectorFieldMapping's doc comment).</summary>
public static class JsonPathWalker
{
    private const int WildcardIndex = -1;

    public static void SetValue(JsonObject root, string path, JsonNode? value)
    {
        var segments = SplitSegments(path);
        JsonObject current = root;

        for (var i = 0; i < segments.Count; i++)
        {
            var (name, index) = segments[i];
            var isLast = i == segments.Count - 1;

            if (index is null)
            {
                if (isLast)
                {
                    current[name] = value;
                }
                else
                {
                    if (current[name] is not JsonObject child)
                    {
                        child = new JsonObject();
                        current[name] = child;
                    }
                    current = child;
                }
            }
            else
            {
                if (current[name] is not JsonArray array)
                {
                    array = new JsonArray();
                    current[name] = array;
                }
                while (array.Count <= index.Value)
                    array.Add(new JsonObject());

                if (isLast)
                {
                    array[index.Value] = value;
                }
                else
                {
                    if (array[index.Value] is not JsonObject child)
                    {
                        child = new JsonObject();
                        array[index.Value] = child;
                    }
                    current = child;
                }
            }
        }
    }

    public static List<JsonElement> GetValues(JsonElement root, string path)
    {
        var segments = SplitSegments(path);
        return Walk(root, segments, 0);
    }

    /// <summary>Number of "[]" wildcards in a path — used to find the most deeply-nested
    /// Inbound mapping, which drives how many quote rows a response produces.</summary>
    public static int CountWildcards(string path) => SplitSegments(path).Count(s => s.Index == WildcardIndex);

    /// <summary>Enumerates every concrete index tuple a wildcarded path resolves to, e.g.
    /// "payload[].rates[].price" against a 2-carrier, {3,5}-rate response yields
    /// [[0,0],[0,1],[0,2],[1,0],...,[1,4]] — one tuple per quote row. Sibling fields at a
    /// shallower nesting (e.g. "payload[].carrierName", one wildcard) are then resolved against
    /// just the tuple's first index via ResolveIndices, so a carrier-level field correctly
    /// broadcasts across all of that carrier's nested rates instead of being index-misaligned.</summary>
    public static List<int[]> GetIndexTuples(JsonElement root, string path)
    {
        var segments = SplitSegments(path);
        var results = new List<int[]>();
        WalkTuples(root, segments, 0, new List<int>(), results);
        return results;
    }

    private static void WalkTuples(JsonElement current, List<(string Name, int? Index)> segments, int segmentIndex, List<int> acc, List<int[]> results)
    {
        if (segmentIndex >= segments.Count)
        {
            results.Add(acc.ToArray());
            return;
        }

        var (name, index) = segments[segmentIndex];
        if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(name, out var next))
            return;

        if (index is null)
        {
            WalkTuples(next, segments, segmentIndex + 1, acc, results);
            return;
        }

        if (next.ValueKind != JsonValueKind.Array)
            return;

        if (index.Value == WildcardIndex)
        {
            for (var i = 0; i < next.GetArrayLength(); i++)
            {
                acc.Add(i);
                WalkTuples(next[i], segments, segmentIndex + 1, acc, results);
                acc.RemoveAt(acc.Count - 1);
            }
        }
        else if (index.Value < next.GetArrayLength())
        {
            WalkTuples(next[index.Value], segments, segmentIndex + 1, acc, results);
        }
    }

    /// <summary>Replaces each "[]" in a path, left to right, with the tuple's indices — only as
    /// many as the path itself has wildcards for. A shallower mapping (fewer wildcards than the
    /// driving tuple) simply consumes a prefix of the tuple, e.g. "payload[].carrierName" against
    /// tuple [1,3] resolves to "payload[1].carrierName", ignoring the rates-level index 3.</summary>
    public static string ResolveIndices(string path, int[] tuple)
    {
        var result = path;
        foreach (var t in tuple)
        {
            var pos = result.IndexOf("[]", StringComparison.Ordinal);
            if (pos < 0) break;
            result = result[..pos] + "[" + t.ToString(CultureInfo.InvariantCulture) + "]" + result[(pos + 2)..];
        }
        return result;
    }

    /// <summary>Fetches a single value from a fully-resolved (no remaining wildcards) path —
    /// null if the path doesn't exist in this particular response.</summary>
    public static JsonElement? GetSingleValue(JsonElement root, string resolvedPath)
    {
        var values = GetValues(root, resolvedPath);
        return values.Count > 0 ? values[0] : null;
    }

    private static List<JsonElement> Walk(JsonElement current, List<(string Name, int? Index)> segments, int segmentIndex)
    {
        if (segmentIndex >= segments.Count)
            return new List<JsonElement> { current };

        var (name, index) = segments[segmentIndex];

        if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(name, out var next))
            return new List<JsonElement>();

        if (index is null)
            return Walk(next, segments, segmentIndex + 1);

        if (next.ValueKind != JsonValueKind.Array)
            return new List<JsonElement>();

        var results = new List<JsonElement>();
        var arrayLength = next.GetArrayLength();

        if (index.Value == WildcardIndex)
        {
            for (var i = 0; i < arrayLength; i++)
                results.AddRange(Walk(next[i], segments, segmentIndex + 1));
        }
        else if (index.Value < arrayLength)
        {
            results.AddRange(Walk(next[index.Value], segments, segmentIndex + 1));
        }

        return results;
    }

    /// <summary>Splits "packages[].weight" into [("packages", wildcard), ("weight", null)];
    /// "packages[0].weight" into [("packages", 0), ("weight", null)]; "destination.zip" into
    /// [("destination", null), ("zip", null)].</summary>
    private static List<(string Name, int? Index)> SplitSegments(string path)
    {
        var result = new List<(string, int?)>();
        foreach (var rawSegment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            var bracketStart = rawSegment.IndexOf('[');
            if (bracketStart < 0)
            {
                result.Add((rawSegment, null));
                continue;
            }

            var name = rawSegment[..bracketStart];
            var closeBracket = rawSegment.IndexOf(']', bracketStart);
            var indexText = rawSegment[(bracketStart + 1)..closeBracket];
            var index = indexText.Length == 0 ? WildcardIndex : int.Parse(indexText, CultureInfo.InvariantCulture);
            result.Add((name, index));
        }
        return result;
    }
}
