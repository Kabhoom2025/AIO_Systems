namespace LinkShield.Application.DTOs.Risk;

public record BrandMatchResultDto(
    Guid BrandProfileId,
    string BrandName,
    string OfficialDomain,
    string MatchedDomain,
    string MatchType,
    double LevenshteinDistance,
    double JaroWinklerSimilarity,
    bool ContainsAuthKeywords);
