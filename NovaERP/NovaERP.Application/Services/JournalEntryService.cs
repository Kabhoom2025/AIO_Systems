using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class JournalEntryService : IJournalEntryService
{
    private readonly IJournalEntryRepository _repo;
    private readonly IValidator<CreateJournalEntryDto> _createValidator;
    private readonly IValidator<UpdateJournalEntryDto> _updateValidator;

    public JournalEntryService(IJournalEntryRepository repo,
        IValidator<CreateJournalEntryDto> createValidator, IValidator<UpdateJournalEntryDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<JournalEntryDto>> GetAllAsync(int orgId)
    {
        var entries = await _repo.GetAllByOrgAsync(orgId);
        return entries.Select(ToDto).ToList();
    }

    public async Task<JournalEntryDto> GetByIdAsync(int orgId, int id)
    {
        var entry = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"JournalEntry {id} not found");
        return ToDto(entry);
    }

    public async Task<JournalEntryDto> CreateAsync(int orgId, CreateJournalEntryDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var entry = new JournalEntry
        {
            OrganizationId = orgId,
            EntryDate = dto.EntryDate,
            Description = dto.Description,
            Status = "Draft",
            OwnerId = dto.OwnerId,
            Lines = dto.Lines.Select(l => new JournalEntryLine
            {
                LedgerAccountId = l.LedgerAccountId,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description,
                DisplayOrder = l.DisplayOrder
            }).ToList()
        };

        _repo.Add(entry);
        await _repo.SaveChangesAsync();

        // EntryNumber depends on the generated Id, so it's set in a second save — same scheme
        // as SalesOrder.OrderNumber/ProductionOrder.MoNumber.
        entry.EntryNumber = $"JE-{entry.Id:D5}";
        _repo.Update(entry);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, entry.Id) ?? entry;
        return ToDto(reloaded);
    }

    public async Task<JournalEntryDto> UpdateAsync(int orgId, int id, UpdateJournalEntryDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var entry = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"JournalEntry {id} not found");

        if (entry.Status != "Draft")
            throw new InvalidOperationException("Only draft journal entries can be edited.");

        entry.EntryDate = dto.EntryDate;
        entry.Description = dto.Description;
        entry.OwnerId = dto.OwnerId;
        entry.UpdatedDate = DateTime.UtcNow;

        // Lines are owned by the entry and replaced wholesale on update.
        entry.Lines.Clear();
        foreach (var l in dto.Lines)
        {
            entry.Lines.Add(new JournalEntryLine
            {
                LedgerAccountId = l.LedgerAccountId,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description,
                DisplayOrder = l.DisplayOrder
            });
        }

        _repo.Update(entry);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, entry.Id) ?? entry;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var entry = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"JournalEntry {id} not found");

        if (entry.Status != "Draft")
            throw new InvalidOperationException("Only draft journal entries can be deleted.");

        _repo.Remove(entry);
        await _repo.SaveChangesAsync();
    }

    public async Task<JournalEntryDto> PostAsync(int orgId, int id)
    {
        var entry = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"JournalEntry {id} not found");

        if (entry.Status != "Draft")
            throw new InvalidOperationException("Only draft journal entries can be posted.");

        // No balance re-check needed here — Create/Update already guarantee Σ(Debit) ==
        // Σ(Credit) for every persisted entry.
        entry.Status = "Posted";
        entry.UpdatedDate = DateTime.UtcNow;
        _repo.Update(entry);
        await _repo.SaveChangesAsync();

        return ToDto(entry);
    }

    public async Task<JournalEntryDto> VoidAsync(int orgId, int id)
    {
        var entry = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"JournalEntry {id} not found");

        if (entry.Status != "Posted")
            throw new InvalidOperationException("Only posted journal entries can be voided.");

        entry.Status = "Voided";
        entry.UpdatedDate = DateTime.UtcNow;
        _repo.Update(entry);
        await _repo.SaveChangesAsync();

        return ToDto(entry);
    }

    private static JournalEntryDto ToDto(JournalEntry e)
    {
        var lineDtos = e.Lines.OrderBy(l => l.DisplayOrder).Select(l => new JournalEntryLineDto
        {
            Id = l.Id,
            LedgerAccountId = l.LedgerAccountId,
            LedgerAccountName = l.LedgerAccount?.Name ?? string.Empty,
            LedgerAccountCode = l.LedgerAccount?.Code ?? string.Empty,
            Debit = l.Debit,
            Credit = l.Credit,
            Description = l.Description,
            DisplayOrder = l.DisplayOrder
        }).ToList();

        return new JournalEntryDto
        {
            Id = e.Id,
            EntryNumber = e.EntryNumber,
            EntryDate = e.EntryDate,
            Description = e.Description,
            Status = e.Status,
            OwnerId = e.OwnerId,
            OwnerName = e.Owner?.Name ?? string.Empty,
            Lines = lineDtos,
            TotalDebit = lineDtos.Sum(l => l.Debit),
            TotalCredit = lineDtos.Sum(l => l.Credit)
        };
    }
}
