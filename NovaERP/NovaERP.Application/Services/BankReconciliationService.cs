using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class BankReconciliationService : IBankReconciliationService
{
    private readonly IBankReconciliationRepository _repo;
    private readonly ILedgerAccountRepository _ledgerAccountRepo;
    private readonly IJournalEntryRepository _journalRepo;
    private readonly IValidator<CreateBankReconciliationDto> _createValidator;
    private readonly IValidator<UpdateBankReconciliationDto> _updateValidator;
    private readonly IValidator<MatchBankStatementLineDto> _matchValidator;

    public BankReconciliationService(IBankReconciliationRepository repo, ILedgerAccountRepository ledgerAccountRepo,
        IJournalEntryRepository journalRepo,
        IValidator<CreateBankReconciliationDto> createValidator, IValidator<UpdateBankReconciliationDto> updateValidator,
        IValidator<MatchBankStatementLineDto> matchValidator)
    {
        _repo = repo;
        _ledgerAccountRepo = ledgerAccountRepo;
        _journalRepo = journalRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _matchValidator = matchValidator;
    }

    public async Task<List<BankReconciliationDto>> GetAllAsync(int orgId)
    {
        var reconciliations = await _repo.GetAllByOrgAsync(orgId);
        var dtos = new List<BankReconciliationDto>();
        foreach (var r in reconciliations)
            dtos.Add(await ToDtoAsync(orgId, r));
        return dtos;
    }

    public async Task<BankReconciliationDto> GetByIdAsync(int orgId, int id)
    {
        var reconciliation = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"BankReconciliation {id} not found");
        return await ToDtoAsync(orgId, reconciliation);
    }

    public async Task<BankReconciliationDto> CreateAsync(int orgId, CreateBankReconciliationDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var reconciliation = new BankReconciliation
        {
            OrganizationId = orgId,
            LedgerAccountId = dto.LedgerAccountId,
            StatementDate = dto.StatementDate,
            StatementEndingBalance = dto.StatementEndingBalance,
            Status = "Draft",
            OwnerId = dto.OwnerId,
            Lines = dto.Lines.Select(l => new BankStatementLine
            {
                TransactionDate = l.TransactionDate,
                Description = l.Description,
                Amount = l.Amount,
                DisplayOrder = l.DisplayOrder
            }).ToList()
        };

        _repo.Add(reconciliation);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, reconciliation.Id) ?? reconciliation;
        return await ToDtoAsync(orgId, reloaded);
    }

    public async Task<BankReconciliationDto> UpdateAsync(int orgId, int id, UpdateBankReconciliationDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var reconciliation = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"BankReconciliation {id} not found");

        if (reconciliation.Status != "Draft")
            throw new InvalidOperationException("Only draft bank reconciliations can be edited.");

        reconciliation.StatementDate = dto.StatementDate;
        reconciliation.StatementEndingBalance = dto.StatementEndingBalance;
        reconciliation.OwnerId = dto.OwnerId;
        reconciliation.UpdatedDate = DateTime.UtcNow;

        // Lines are owned by the reconciliation and replaced wholesale on update — any
        // existing matches are necessarily lost, since the old line rows are gone.
        reconciliation.Lines.Clear();
        foreach (var l in dto.Lines)
        {
            reconciliation.Lines.Add(new BankStatementLine
            {
                TransactionDate = l.TransactionDate,
                Description = l.Description,
                Amount = l.Amount,
                DisplayOrder = l.DisplayOrder
            });
        }

        _repo.Update(reconciliation);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, reconciliation.Id) ?? reconciliation;
        return await ToDtoAsync(orgId, reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var reconciliation = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"BankReconciliation {id} not found");

        if (reconciliation.Status != "Draft")
            throw new InvalidOperationException("Only draft bank reconciliations can be deleted.");

        _repo.Remove(reconciliation);
        await _repo.SaveChangesAsync();
    }

    public async Task<BankReconciliationDto> MatchLineAsync(int orgId, int reconciliationId, int lineId, MatchBankStatementLineDto dto)
    {
        await _matchValidator.ValidateAndThrowAsync(dto);

        var reconciliation = await _repo.GetByIdAsync(orgId, reconciliationId)
            ?? throw new KeyNotFoundException($"BankReconciliation {reconciliationId} not found");

        if (reconciliation.Status != "Draft")
            throw new InvalidOperationException("Only draft bank reconciliations can have their lines matched.");

        var line = reconciliation.Lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new KeyNotFoundException($"BankStatementLine {lineId} not found on this reconciliation");

        var journalLine = await _journalRepo.GetLineByIdAsync(orgId, dto.JournalEntryLineId)
            ?? throw new KeyNotFoundException($"JournalEntryLine {dto.JournalEntryLineId} not found");

        if (journalLine.LedgerAccountId != reconciliation.LedgerAccountId)
            throw new InvalidOperationException("The journal entry line must belong to the same ledger account as this reconciliation.");

        if (journalLine.JournalEntry.Status != "Posted")
            throw new InvalidOperationException("Only posted journal entry lines can be matched.");

        var signedAmount = journalLine.Debit - journalLine.Credit;
        if (signedAmount != line.Amount)
            throw new InvalidOperationException(
                $"Amount mismatch: statement line is {line.Amount}, journal entry line is {signedAmount}.");

        var alreadyMatched = await _journalRepo.GetUnmatchedPostedLinesByAccountAsync(orgId, reconciliation.LedgerAccountId);
        if (!alreadyMatched.Any(l => l.Id == journalLine.Id))
            throw new InvalidOperationException("This journal entry line is already matched to another statement line.");

        line.MatchedJournalEntryLineId = journalLine.Id;
        _repo.Update(reconciliation);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, reconciliationId) ?? reconciliation;
        return await ToDtoAsync(orgId, reloaded);
    }

    public async Task<BankReconciliationDto> UnmatchLineAsync(int orgId, int reconciliationId, int lineId)
    {
        var reconciliation = await _repo.GetByIdAsync(orgId, reconciliationId)
            ?? throw new KeyNotFoundException($"BankReconciliation {reconciliationId} not found");

        if (reconciliation.Status != "Draft")
            throw new InvalidOperationException("Only draft bank reconciliations can have their lines unmatched.");

        var line = reconciliation.Lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new KeyNotFoundException($"BankStatementLine {lineId} not found on this reconciliation");

        line.MatchedJournalEntryLineId = null;
        _repo.Update(reconciliation);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, reconciliationId) ?? reconciliation;
        return await ToDtoAsync(orgId, reloaded);
    }

    public async Task<BankReconciliationDto> CompleteAsync(int orgId, int id)
    {
        var reconciliation = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"BankReconciliation {id} not found");

        if (reconciliation.Status != "Draft")
            throw new InvalidOperationException("Only draft bank reconciliations can be completed.");

        var unmatchedCount = reconciliation.Lines.Count(l => l.MatchedJournalEntryLineId == null);
        if (unmatchedCount > 0)
            throw new InvalidOperationException($"{unmatchedCount} statement line(s) still need to be matched before completing.");

        // The entire point of a reconciliation: prove the bank statement's ending balance
        // equals the account's actual book balance — a live query, not a DTO-only check.
        var bookBalance = await _ledgerAccountRepo.GetBalanceAsync(orgId, reconciliation.LedgerAccountId);
        if (reconciliation.StatementEndingBalance != bookBalance)
            throw new InvalidOperationException(
                $"Statement ending balance ({reconciliation.StatementEndingBalance}) does not match the book balance ({bookBalance}).");

        reconciliation.Status = "Completed";
        reconciliation.UpdatedDate = DateTime.UtcNow;
        _repo.Update(reconciliation);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, id) ?? reconciliation;
        return await ToDtoAsync(orgId, reloaded);
    }

    public async Task<List<MatchCandidateDto>> GetMatchCandidatesAsync(int orgId, int ledgerAccountId)
    {
        var lines = await _journalRepo.GetUnmatchedPostedLinesByAccountAsync(orgId, ledgerAccountId);
        return lines.Select(l => new MatchCandidateDto
        {
            JournalEntryLineId = l.Id,
            JournalEntryNumber = l.JournalEntry.EntryNumber,
            EntryDate = l.JournalEntry.EntryDate,
            Description = l.Description ?? l.JournalEntry.Description,
            Amount = l.Debit - l.Credit
        }).ToList();
    }

    private async Task<BankReconciliationDto> ToDtoAsync(int orgId, BankReconciliation r)
    {
        var bookBalance = await _ledgerAccountRepo.GetBalanceAsync(orgId, r.LedgerAccountId);

        var lineDtos = r.Lines.OrderBy(l => l.DisplayOrder).Select(l => new BankStatementLineDto
        {
            Id = l.Id,
            TransactionDate = l.TransactionDate,
            Description = l.Description,
            Amount = l.Amount,
            MatchedJournalEntryLineId = l.MatchedJournalEntryLineId,
            DisplayOrder = l.DisplayOrder,
            IsMatched = l.MatchedJournalEntryLineId != null
        }).ToList();

        return new BankReconciliationDto
        {
            Id = r.Id,
            LedgerAccountId = r.LedgerAccountId,
            LedgerAccountName = r.LedgerAccount?.Name ?? string.Empty,
            StatementDate = r.StatementDate,
            StatementEndingBalance = r.StatementEndingBalance,
            Status = r.Status,
            OwnerId = r.OwnerId,
            OwnerName = r.Owner?.Name ?? string.Empty,
            Lines = lineDtos,
            BookBalance = bookBalance,
            Difference = r.StatementEndingBalance - bookBalance,
            UnmatchedCount = lineDtos.Count(l => !l.IsMatched)
        };
    }
}
