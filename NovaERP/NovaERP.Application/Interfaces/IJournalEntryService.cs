using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IJournalEntryService
{
    Task<List<JournalEntryDto>> GetAllAsync(int orgId);
    Task<JournalEntryDto> GetByIdAsync(int orgId, int id);
    Task<JournalEntryDto> CreateAsync(int orgId, CreateJournalEntryDto dto);
    Task<JournalEntryDto> UpdateAsync(int orgId, int id, UpdateJournalEntryDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<JournalEntryDto> PostAsync(int orgId, int id);
    Task<JournalEntryDto> VoidAsync(int orgId, int id);
}
