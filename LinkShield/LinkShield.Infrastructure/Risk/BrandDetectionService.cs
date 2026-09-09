using LinkShield.Application.Common;
using LinkShield.Application.DTOs.Risk;
using LinkShield.Application.Interfaces;
using LinkShield.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkShield.Infrastructure.Risk;

/// <summary>
/// Typosquatters rarely register an exact near-miss of "paypal.com" — they register things
/// like "paypa1-secure-login.tk", where the brand name is one hyphen-separated token among
/// several. Comparing the whole registrable domain string against the whole official domain
/// misses this entirely (the strings are simply different lengths/shapes). So every
/// hyphen/dot-separated label in the scanned domain is compared individually against each
/// brand's name and official-domain label, and the strongest match wins.
/// </summary>
public class BrandDetectionService : IBrandDetectionService
{
    private static readonly string[] AuthKeywords =
    [
        "login", "signin", "verify", "verification", "account", "password", "secure", "update", "confirm"
    ];

    private const int TyposquatDistanceThreshold = 2;
    private const double HomoglyphSimilarityThreshold = 0.90;

    private readonly LinkShieldDbContext _db;

    public BrandDetectionService(LinkShieldDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<BrandMatchResultDto>> DetectAsync(string registeredDomain, string normalizedUrl, CancellationToken ct = default)
    {
        var brands = await _db.BrandProfiles.Where(b => b.IsEnabled).ToListAsync(ct);
        var lowerUrl = normalizedUrl.ToLowerInvariant();
        var lowerDomain = registeredDomain.ToLowerInvariant();
        var hasAuthKeyword = AuthKeywords.Any(lowerUrl.Contains);

        var domainLabel = StripTld(lowerDomain);
        var candidates = domainLabel.Split('-', '.')
            .Where(t => t.Length > 0)
            .Append(domainLabel)
            .Distinct()
            .ToList();

        var matches = new List<BrandMatchResultDto>();

        foreach (var brand in brands)
        {
            var officialDomain = brand.OfficialDomain.ToLowerInvariant();
            if (lowerDomain == officialDomain) continue; // the real domain — nothing to flag

            var officialLabel = StripTld(officialDomain);
            var brandNameLower = brand.BrandName.ToLowerInvariant();
            var targets = new[] { officialLabel, brandNameLower }.Distinct();

            var best = candidates
                .SelectMany(candidate => targets.Select(target => (
                    Distance: StringSimilarity.LevenshteinDistance(candidate, target),
                    Similarity: StringSimilarity.JaroWinklerSimilarity(candidate, target),
                    HasNonAscii: candidate.Any(c => !char.IsAscii(c) && char.IsLetter(c)))))
                .OrderBy(r => r.Distance)
                .ThenByDescending(r => r.Similarity)
                .First();

            string? matchType = null;
            if (best.HasNonAscii && best.Similarity >= HomoglyphSimilarityThreshold)
                matchType = "Homoglyph";
            else if (best.Distance == 0)
                matchType = "BrandNameInDomain"; // exact brand name/label used as a token in a non-official domain
            else if (best.Distance <= TyposquatDistanceThreshold)
                matchType = "Typosquat";
            else if (lowerUrl.Contains(brandNameLower) && hasAuthKeyword)
                matchType = "KeywordCombo";

            if (matchType is null) continue;

            matches.Add(new BrandMatchResultDto(
                brand.Id, brand.BrandName, brand.OfficialDomain, registeredDomain, matchType,
                best.Distance, best.Similarity, hasAuthKeyword));
        }

        return matches;
    }

    private static string StripTld(string domain)
    {
        var lastDot = domain.LastIndexOf('.');
        return lastDot < 0 ? domain : domain[..lastDot];
    }
}
