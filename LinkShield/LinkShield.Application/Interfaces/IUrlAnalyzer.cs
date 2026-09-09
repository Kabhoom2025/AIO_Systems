using LinkShield.Application.DTOs.Scans;

namespace LinkShield.Application.Interfaces;

/// <summary>Pure, stateless URL-structure analysis — no network calls, no persistence.
/// Safe to call on any string a caller provides, including ones that will never be
/// fetched (spec section 5 is about the URL text itself, not what it points to).</summary>
public interface IUrlAnalyzer
{
    UrlAnalysisResultDto Analyze(string rawUrl);
}
