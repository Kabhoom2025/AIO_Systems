using FoodOrder.Application.DTOs;

namespace FoodOrder.Application.Interfaces.Services;

public interface ILedgerService
{
    Task<IReadOnlyList<LedgerEntryDTO>> GetEntriesAsync(DateTime from, DateTime to);
    Task<LedgerSummaryDTO> GetDailySummaryAsync(DateTime date);
    Task<LedgerEntryDTO> CreateAsync(CreateLedgerEntryRequest dto, int userId);
    Task DeleteAsync(int id);
}
