namespace LinkShield.Application.Common;

/// <summary>Registrable-domain / subdomain-count logic shared by UrlAnalyzer and anything else
/// that needs "what's the registrable domain for this host" without a full public-suffix-list
/// package. See UrlAnalyzer's class doc for why this heuristic exists and its limitation.</summary>
public static class DomainNameHelper
{
    private static readonly string[] MultiPartSuffixes =
    [
        "co.uk", "org.uk", "ac.uk", "gov.uk", "co.jp", "co.in", "com.au", "com.br",
        "com.cn", "com.mx", "com.tr", "co.nz", "co.za", "com.sg"
    ];

    public static string GetRegisteredDomain(string host)
    {
        var labels = host.Split('.');
        if (labels.Length <= 2) return host;

        foreach (var suffix in MultiPartSuffixes)
        {
            if (host.EndsWith("." + suffix, StringComparison.OrdinalIgnoreCase))
            {
                var suffixLabelCount = suffix.Count(c => c == '.') + 1;
                var take = Math.Min(labels.Length, suffixLabelCount + 1);
                return string.Join('.', labels.Skip(labels.Length - take));
            }
        }

        return string.Join('.', labels.Skip(labels.Length - 2));
    }

    public static int CountSubdomains(string host, string registeredDomain)
    {
        if (host.Equals(registeredDomain, StringComparison.OrdinalIgnoreCase)) return 0;
        var prefix = host[..^(registeredDomain.Length + 1)];
        return prefix.Length == 0 ? 0 : prefix.Count(c => c == '.') + 1;
    }

    public static string GetTld(string registeredDomain)
    {
        var labels = registeredDomain.Split('.');
        return labels.Length == 0 ? string.Empty : labels[^1];
    }
}
