using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IBankReconciliationService
{
    Task<List<BankReconciliationDto>> GetAllAsync(int orgId);
    Task<BankReconciliationDto> GetByIdAsync(int orgId, int id);
    Task<BankReconciliationDto> CreateAsync(int orgId, CreateBankReconciliationDto dto);
    Task<BankReconciliationDto> UpdateAsync(int orgId, int id, UpdateBankReconciliationDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<BankReconciliationDto> MatchLineAsync(int orgId, int reconciliationId, int lineId, MatchBankStatementLineDto dto);
    Task<BankReconciliationDto> UnmatchLineAsync(int orgId, int reconciliationId, int lineId);
    Task<BankReconciliationDto> CompleteAsync(int orgId, int id);
    Task<List<MatchCandidateDto>> GetMatchCandidatesAsync(int orgId, int ledgerAccountId);
}
