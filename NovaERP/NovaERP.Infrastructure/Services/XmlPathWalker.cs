using System.Globalization;
using System.Xml.Linq;

namespace NovaERP.Infrastructure.Services;

/// <summary>XML analog of JsonPathWalker's read side — same dot/bracket path convention
/// ("ratequote.ratequoteline[].chrg"), but walking XElement children instead of JSON
/// properties/arrays. Repeated sibling elements sharing a tag name are this format's "array":
/// a bare segment name addresses the first matching child, "[]" iterates every matching child,
/// "[n]" addresses the nth. Read-only (no SetValue) — XML request bodies aren't built from
/// mappings in this pass, only XML responses are parsed for Inbound mappings.</summary>
public static class XmlPathWalker
{
    private const int WildcardIndex = -1;

    public static List<string> GetValues(XElement root, string path)
    {
        var segments = SplitSegments(path);
        return Walk(root, segments, 0);
    }

    /// <summary>Number of "[]" wildcards in a path — used to find the most deeply-nested
    /// Inbound mapping, which drives how many quote rows a response produces.</summary>
    public static int CountWildcards(string path) => SplitSegments(path).Count(s => s.Index == WildcardIndex);

    /// <summary>Enumerates every concrete index tuple a wildcarded path resolves to — mirrors
    /// JsonPathWalker.GetIndexTuples exactly, just walking XElement children instead of JSON
    /// array elements.</summary>
    public static List<int[]> GetIndexTuples(XElement root, string path)
    {
        var segments = SplitSegments(path);
        var results = new List<int[]>();
        WalkTuples(root, segments, 0, new List<int>(), results);
        return results;
    }

    /// <summary>Replaces each "[]" in a path, left to right, with the tuple's indices — same
    /// convention/implementation as JsonPathWalker.ResolveIndices.</summary>
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

    /// <summary>Fetches a single value (an element's inner text) from a fully-resolved (no
    /// remaining wildcards) path — null if the path doesn't exist in this particular response.</summary>
    public static string? GetSingleValue(XElement root, string resolvedPath)
    {
        var values = GetValues(root, resolvedPath);
        return values.Count > 0 ? values[0] : null;
    }

    private static List<string> Walk(XElement current, List<(string Name, int? Index)> segments, int segmentIndex)
    {
        if (segmentIndex >= segments.Count)
            return new List<string> { current.Value };

        var (name, index) = segments[segmentIndex];
        var matches = current.Elements(name).ToList();
        if (matches.Count == 0) return new List<string>();

        if (index is null)
            return Walk(matches[0], segments, segmentIndex + 1);

        if (index.Value == WildcardIndex)
        {
            var results = new List<string>();
            foreach (var m in matches)
                results.AddRange(Walk(m, segments, segmentIndex + 1));
            return results;
        }

        return index.Value < matches.Count ? Walk(matches[index.Value], segments, segmentIndex + 1) : new List<string>();
    }

    private static void WalkTuples(XElement current, List<(string Name, int? Index)> segments, int segmentIndex, List<int> acc, List<int[]> results)
    {
        if (segmentIndex >= segments.Count)
        {
            results.Add(acc.ToArray());
            return;
        }

        var (name, index) = segments[segmentIndex];
        var matches = current.Elements(name).ToList();
        if (matches.Count == 0) return;

        if (index is null)
        {
            WalkTuples(matches[0], segments, segmentIndex + 1, acc, results);
            return;
        }

        if (index.Value == WildcardIndex)
        {
            for (var i = 0; i < matches.Count; i++)
            {
                acc.Add(i);
                WalkTuples(matches[i], segments, segmentIndex + 1, acc, results);
                acc.RemoveAt(acc.Count - 1);
            }
        }
        else if (index.Value < matches.Count)
        {
            WalkTuples(matches[index.Value], segments, segmentIndex + 1, acc, results);
        }
    }

    /// <summary>Splits "ratequoteline[].chrg" into [("ratequoteline", wildcard), ("chrg", null)];
    /// same convention as JsonPathWalker.SplitSegments.</summary>
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
