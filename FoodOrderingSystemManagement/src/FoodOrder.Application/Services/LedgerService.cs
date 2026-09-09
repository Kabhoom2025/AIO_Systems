using AutoMapper;
using FoodOrder.Application.DTOs;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class LedgerService : ILedgerService
{
    private readonly ILedgerRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILedgerNotifier _notifier;

    public LedgerService(ILedgerRepository repo, IMapper mapper, ILedgerNotifier notifier)
    {
        _repo     = repo;
        _mapper   = mapper;
        _notifier = notifier;
    }

    public async Task<IReadOnlyList<LedgerEntryDTO>> GetEntriesAsync(DateTime from, DateTime to)
    {
        var entries = await _repo.GetByDateRangeAsync(from.Date, to.Date.AddDays(1).AddTicks(-1));
        var dtos = _mapper.Map<List<LedgerEntryDTO>>(entries);

        // Compute running balance oldest → newest
        decimal running = 0;
        foreach (var dto in dtos)
        {
            running += dto.Type == "Credit" ? dto.Amount : -dto.Amount;
            dto.RunningBalance = running;
        }

        return dtos;
    }

    public async Task<LedgerSummaryDTO> GetDailySummaryAsync(DateTime date)
    {
        var entries = await _repo.GetByDateRangeAsync(date.Date, date.Date.AddDays(1).AddTicks(-1));

        var credit = entries.Where(e => e.Type == "Credit").Sum(e => e.Amount);
        var debit  = entries.Where(e => e.Type == "Debit").Sum(e => e.Amount);

        return new LedgerSummaryDTO
        {
            Date        = date.Date,
            TotalCredit = credit,
            TotalDebit  = debit,
            Net         = credit - debit,
            EntryCount  = entries.Count,
        };
    }

    public async Task<LedgerEntryDTO> CreateAsync(CreateLedgerEntryRequest dto, int userId)
    {
        var entry = _mapper.Map<Domain.Entities.LedgerEntry>(dto);
        entry.CreatedById = userId;
        entry.CreatedDate = DateTime.UtcNow;
        entry.Date        = DateTime.SpecifyKind(entry.Date, DateTimeKind.Utc);

        await _repo.AddAsync(entry);
        await _repo.SaveChangesAsync();

        var result = _mapper.Map<LedgerEntryDTO>(entry);
        await _notifier.NotifyEntryCreatedAsync(result);
        return result;
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException("LedgerEntry", id);
        await _repo.DeleteAsync(id);
        await _repo.SaveChangesAsync();
    }
}
