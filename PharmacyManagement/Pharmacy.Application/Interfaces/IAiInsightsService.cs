using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IAiInsightsService
{
    Task<List<ReorderSuggestionDto>> GetReorderSuggestionsAsync(int orgId);
    Task<List<ExpiryRiskDto>> GetExpiryRiskAsync(int orgId);
}
