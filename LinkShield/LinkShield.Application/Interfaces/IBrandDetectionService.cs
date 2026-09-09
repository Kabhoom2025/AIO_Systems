using LinkShield.Application.DTOs.Risk;

namespace LinkShield.Application.Interfaces;

/// <summary>Compares the scanned domain (and full URL text) against configurable BrandProfile
/// rows for typosquatting, homoglyph look-alikes, and brand-name+auth-keyword combinations
/// (spec section 11). Brand list is data, not code — adding a brand never touches this logic.</summary>
public interface IBrandDetectionService
{
    Task<IReadOnlyList<BrandMatchResultDto>> DetectAsync(string registeredDomain, string normalizedUrl, CancellationToken ct = default);
}
