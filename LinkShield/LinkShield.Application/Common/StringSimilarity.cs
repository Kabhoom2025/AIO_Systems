namespace LinkShield.Application.Common;

/// <summary>Pure string-distance algorithms used for typosquat/brand-impersonation detection
/// (spec section 11). No dependencies, fully unit-testable in isolation.</summary>
public static class StringSimilarity
{
    public static int LevenshteinDistance(string a, string b)
    {
        if (a == b) return 0;
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        var previousRow = new int[b.Length + 1];
        var currentRow = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++) previousRow[j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            currentRow[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                currentRow[j] = Math.Min(Math.Min(currentRow[j - 1] + 1, previousRow[j] + 1), previousRow[j - 1] + cost);
            }
            (previousRow, currentRow) = (currentRow, previousRow);
        }

        return previousRow[b.Length];
    }

    /// <summary>Jaro-Winkler similarity in [0, 1] — 1.0 means identical.</summary>
    public static double JaroWinklerSimilarity(string a, string b)
    {
        var jaro = JaroSimilarity(a, b);
        if (jaro <= 0.7) return jaro;

        var prefixLength = 0;
        var maxPrefix = Math.Min(4, Math.Min(a.Length, b.Length));
        while (prefixLength < maxPrefix && a[prefixLength] == b[prefixLength]) prefixLength++;

        return jaro + prefixLength * 0.1 * (1 - jaro);
    }

    private static double JaroSimilarity(string a, string b)
    {
        if (a.Length == 0 && b.Length == 0) return 1.0;
        if (a.Length == 0 || b.Length == 0) return 0.0;

        var matchWindow = Math.Max(a.Length, b.Length) / 2 - 1;
        if (matchWindow < 0) matchWindow = 0;

        var aMatched = new bool[a.Length];
        var bMatched = new bool[b.Length];
        var matches = 0;

        for (var i = 0; i < a.Length; i++)
        {
            var start = Math.Max(0, i - matchWindow);
            var end = Math.Min(b.Length - 1, i + matchWindow);
            for (var j = start; j <= end; j++)
            {
                if (bMatched[j] || a[i] != b[j]) continue;
                aMatched[i] = true;
                bMatched[j] = true;
                matches++;
                break;
            }
        }

        if (matches == 0) return 0.0;

        var transpositions = 0;
        var bIndex = 0;
        for (var i = 0; i < a.Length; i++)
        {
            if (!aMatched[i]) continue;
            while (!bMatched[bIndex]) bIndex++;
            if (a[i] != b[bIndex]) transpositions++;
            bIndex++;
        }

        var m = (double)matches;
        return (m / a.Length + m / b.Length + (m - transpositions / 2.0) / m) / 3.0;
    }
}
