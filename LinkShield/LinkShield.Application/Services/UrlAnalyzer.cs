using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using LinkShield.Application.Common;
using LinkShield.Application.DTOs.Scans;
using LinkShield.Application.Interfaces;

namespace LinkShield.Application.Services;

/// <summary>
/// Computes the spec-section-5 structural URL signals. Deliberately network-free: this only
/// looks at the URL string as text. Registrable-domain / subdomain-count uses
/// <see cref="DomainNameHelper"/> — a small built-in multi-part-suffix list rather than a full
/// public suffix list (tldextract-equivalent) — good enough for the common cases, revisit if a
/// public-suffix-list package is added later.
/// </summary>
public class UrlAnalyzer : IUrlAnalyzer
{
    private static readonly HashSet<string> UrlShortenerHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "bit.ly", "tinyurl.com", "goo.gl", "t.co", "ow.ly", "is.gd", "buff.ly",
        "adf.ly", "shorte.st", "rebrand.ly", "cutt.ly", "tiny.cc", "rb.gy"
    };

    private static readonly HashSet<string> SuspiciousTlds = new(StringComparer.OrdinalIgnoreCase)
    {
        "tk", "ml", "ga", "cf", "gq", "top", "xyz", "club", "work", "click",
        "loan", "men", "review", "download", "racing", "win", "bid", "stream"
    };

    private static readonly string[] SuspiciousKeywords =
    [
        "login", "signin", "log-in", "verify", "verification", "account", "password",
        "secure", "security", "update", "confirm", "banking", "billing", "payment",
        "webscr", "suspend", "unlock", "urgent", "expire"
    ];

    private static readonly string[] SuspiciousParameterNames =
    [
        "redirect", "redir", "url", "next", "return", "returnurl", "return_url",
        "continue", "dest", "destination", "goto", "target"
    ];

    private static readonly string[] SuspiciousFileExtensions =
    [
        ".exe", ".scr", ".bat", ".cmd", ".js", ".vbs", ".apk", ".msi", ".jar", ".ps1"
    ];

    private static readonly Regex PercentEncodingPattern = new("%[0-9A-Fa-f]{2}", RegexOptions.Compiled);
    private static readonly IdnMapping IdnMapping = new();

    public UrlAnalysisResultDto Analyze(string rawUrl)
    {
        var normalizedUrl = Normalize(rawUrl);
        var uri = new Uri(normalizedUrl);

        var host = uri.Host;
        var path = uri.AbsolutePath;
        var query = uri.Query;

        var isIpAddressHost = IPAddress.TryParse(host, out _);
        var hasPunycode = host.Split('.').Any(label => label.StartsWith("xn--", StringComparison.OrdinalIgnoreCase));
        var hasHomoglyphs = DetectHomoglyphs(host);
        var registeredDomain = DomainNameHelper.GetRegisteredDomain(host);
        var subdomainCount = isIpAddressHost ? 0 : DomainNameHelper.CountSubdomains(host, registeredDomain);
        var tld = isIpAddressHost ? string.Empty : DomainNameHelper.GetTld(registeredDomain);

        var hasUrlEncoding = PercentEncodingPattern.IsMatch(normalizedUrl);
        var hasDoubleEncoding = DetectDoubleEncoding(normalizedUrl);
        var isShortenedUrl = UrlShortenerHosts.Contains(host);
        var hasSuspiciousTld = SuspiciousTlds.Contains(tld);

        var suspiciousParameters = ParseQueryParameterNames(query)
            .Where(k => SuspiciousParameterNames.Contains(k, StringComparer.OrdinalIgnoreCase))
            .ToList();
        var hasRedirectParameter = suspiciousParameters.Count > 0;

        var hasSuspiciousFileExtension = SuspiciousFileExtensions.Any(ext =>
            path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));

        var lowerUrl = normalizedUrl.ToLowerInvariant();
        var suspiciousKeywords = SuspiciousKeywords.Where(lowerUrl.Contains).ToList();

        var result = new UrlAnalysisResultDto(
            OriginalUrl: rawUrl,
            NormalizedUrl: normalizedUrl,
            UrlLength: normalizedUrl.Length,
            DomainLength: host.Length,
            PathLength: path.Length,
            QueryLength: query.Length,
            SubdomainCount: subdomainCount,
            DotCount: host.Count(c => c == '.'),
            HyphenCount: host.Count(c => c == '-'),
            DigitCount: host.Count(char.IsDigit),
            SpecialCharCount: normalizedUrl.Count(c => !char.IsLetterOrDigit(c) && c is not ('.' or '/' or ':' or '-')),
            IsIpAddressHost: isIpAddressHost,
            IsHttps: uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase),
            HasUrlEncoding: hasUrlEncoding,
            HasDoubleEncoding: hasDoubleEncoding,
            HasPunycode: hasPunycode,
            HasHomoglyphs: hasHomoglyphs,
            IsShortenedUrl: isShortenedUrl,
            HasSuspiciousTld: hasSuspiciousTld,
            HasRedirectParameter: hasRedirectParameter,
            HasSuspiciousFileExtension: hasSuspiciousFileExtension,
            SuspiciousKeywords: suspiciousKeywords,
            SuspiciousParameters: suspiciousParameters,
            UrlAnalysisScore: 0);

        return result with { UrlAnalysisScore = ComputeStageScore(result) };
    }

    private static string Normalize(string rawUrl)
    {
        var trimmed = rawUrl.Trim();
        var uri = new Uri(trimmed, UriKind.Absolute);

        var builder = new UriBuilder(uri) { Host = uri.Host.ToLowerInvariant() };
        if ((builder.Scheme == "http" && builder.Port == 80) || (builder.Scheme == "https" && builder.Port == 443))
            builder.Port = -1;

        return builder.Uri.ToString();
    }

    private static bool DetectDoubleEncoding(string url)
    {
        // "%25" is a percent sign that has itself been percent-encoded — a strong signal that
        // something in the URL was encoded twice (e.g. %2570 = double-encoded 'p').
        return url.Contains("%25", StringComparison.OrdinalIgnoreCase) &&
               Regex.IsMatch(url, "%25[0-9A-Fa-f]{2}");
    }

    private static bool DetectHomoglyphs(string host)
    {
        if (host.StartsWith("xn--", StringComparison.OrdinalIgnoreCase) ||
            host.Split('.').Any(l => l.StartsWith("xn--", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                host = IdnMapping.GetUnicode(host);
            }
            catch (ArgumentException)
            {
                // Malformed punycode — leave as-is, ASCII-only check below will simply pass.
            }
        }

        // Heuristic: a registrable domain mixing ASCII Latin letters with any non-ASCII letter
        // is a classic homoglyph/typosquat pattern (e.g. Latin "a" + Cyrillic "а").
        var hasAscii = host.Any(c => char.IsAscii(c) && char.IsLetter(c));
        var hasNonAscii = host.Any(c => !char.IsAscii(c) && char.IsLetter(c));
        return hasAscii && hasNonAscii;
    }

    private static IEnumerable<string> ParseQueryParameterNames(string query)
    {
        if (string.IsNullOrEmpty(query)) return [];

        var trimmed = query.TrimStart('?');
        if (trimmed.Length == 0) return [];

        return trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(pair => pair.Split('=', 2)[0])
            .Where(name => name.Length > 0);
    }

    private static int ComputeStageScore(UrlAnalysisResultDto r)
    {
        var score = 0;
        if (r.IsIpAddressHost) score += 25;
        if (!r.IsHttps) score += 10;
        if (r.HasDoubleEncoding) score += 15;
        if (r.HasPunycode) score += 15;
        if (r.HasHomoglyphs) score += 15;
        if (r.IsShortenedUrl) score += 10;
        if (r.HasSuspiciousTld) score += 15;
        if (r.HasSuspiciousFileExtension) score += 15;
        if (r.HasRedirectParameter) score += 10;
        if (r.SuspiciousKeywords.Count > 0) score += 10;
        return Math.Min(score, 100);
    }
}
