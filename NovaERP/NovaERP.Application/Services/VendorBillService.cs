using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class VendorBillService : IVendorBillService
{
    private readonly IVendorBillRepository _repo;
    private readonly IJournalEntryRepository _journalRepo;
    private readonly IValidator<CreateVendorBillDto> _createValidator;
    private readonly IValidator<UpdateVendorBillDto> _updateValidator;
    private readonly IValidator<PayVendorBillDto> _payValidator;

    public VendorBillService(IVendorBillRepository repo, IJournalEntryRepository journalRepo,
        IValidator<CreateVendorBillDto> createValidator, IValidator<UpdateVendorBillDto> updateValidator,
        IValidator<PayVendorBillDto> payValidator)
    {
        _repo = repo;
        _journalRepo = journalRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _payValidator = payValidator;
    }

    public async Task<List<VendorBillDto>> GetAllAsync(int orgId)
    {
        var bills = await _repo.GetAllByOrgAsync(orgId);
        return bills.Select(ToDto).ToList();
    }

    public async Task<VendorBillDto> GetByIdAsync(int orgId, int id)
    {
        var bill = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"VendorBill {id} not found");
        return ToDto(bill);
    }

    public async Task<VendorBillDto> CreateAsync(int orgId, CreateVendorBillDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var bill = new VendorBill
        {
            OrganizationId = orgId,
            VendorId = dto.VendorId,
            PayableLedgerAccountId = dto.PayableLedgerAccountId,
            BillDate = dto.BillDate,
            DueDate = dto.DueDate,
            Status = "Draft",
            OwnerId = dto.OwnerId,
            Lines = dto.Lines.Select(l => new VendorBillLine
            {
                LedgerAccountId = l.LedgerAccountId,
                Description = l.Description,
                Amount = l.Amount,
                DisplayOrder = l.DisplayOrder
            }).ToList()
        };

        _repo.Add(bill);
        await _repo.SaveChangesAsync();

        // BillNumber depends on the generated Id, so it's set in a second save — same scheme
        // as SalesOrder.OrderNumber/JournalEntry.EntryNumber.
        bill.BillNumber = $"BILL-{bill.Id:D5}";
        _repo.Update(bill);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, bill.Id) ?? bill;
        return ToDto(reloaded);
    }

    public async Task<VendorBillDto> UpdateAsync(int orgId, int id, UpdateVendorBillDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var bill = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"VendorBill {id} not found");

        if (bill.Status != "Draft")
            throw new InvalidOperationException("Only draft vendor bills can be edited.");

        bill.PayableLedgerAccountId = dto.PayableLedgerAccountId;
        bill.BillDate = dto.BillDate;
        bill.DueDate = dto.DueDate;
        bill.OwnerId = dto.OwnerId;
        bill.UpdatedDate = DateTime.UtcNow;

        // Lines are owned by the bill and replaced wholesale on update.
        bill.Lines.Clear();
        foreach (var l in dto.Lines)
        {
            bill.Lines.Add(new VendorBillLine
            {
                LedgerAccountId = l.LedgerAccountId,
                Description = l.Description,
                Amount = l.Amount,
                DisplayOrder = l.DisplayOrder
            });
        }

        _repo.Update(bill);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, bill.Id) ?? bill;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var bill = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"VendorBill {id} not found");

        if (bill.Status != "Draft")
            throw new InvalidOperationException("Only draft vendor bills can be deleted.");

        _repo.Remove(bill);
        await _repo.SaveChangesAsync();
    }

    public async Task<VendorBillDto> ApproveAsync(int orgId, int id)
    {
        var bill = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"VendorBill {id} not found");

        if (bill.Status != "Draft")
            throw new InvalidOperationException("Only draft vendor bills can be approved.");

        var total = bill.Lines.Sum(l => l.Amount);
        var entry = new JournalEntry
        {
            OrganizationId = orgId,
            EntryDate = DateTime.UtcNow.Date,
            Description = $"Vendor Bill {bill.BillNumber}",
            Status = "Posted",
            OwnerId = bill.OwnerId,
            Lines = bill.Lines.Select((l, i) => new JournalEntryLine
            {
                LedgerAccountId = l.LedgerAccountId,
                Debit = l.Amount,
                Credit = 0m,
                DisplayOrder = i + 1
            }).ToList()
        };
        entry.Lines.Add(new JournalEntryLine
        {
            LedgerAccountId = bill.PayableLedgerAccountId,
            Debit = 0m,
            Credit = total,
            DisplayOrder = entry.Lines.Count + 1
        });

        _journalRepo.Add(entry);
        await _journalRepo.SaveChangesAsync();

        entry.EntryNumber = $"JE-{entry.Id:D5}";
        _journalRepo.Update(entry);
        await _journalRepo.SaveChangesAsync();

        bill.Status = "Approved";
        bill.PostedJournalEntryId = entry.Id;
        bill.UpdatedDate = DateTime.UtcNow;
        _repo.Update(bill);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, bill.Id) ?? bill;
        return ToDto(reloaded);
    }

    public async Task<VendorBillDto> PayAsync(int orgId, int id, PayVendorBillDto dto)
    {
        await _payValidator.ValidateAndThrowAsync(dto);

        var bill = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"VendorBill {id} not found");

        if (bill.Status != "Approved")
            throw new InvalidOperationException("Only approved vendor bills can be paid.");

        var total = bill.Lines.Sum(l => l.Amount);
        var entry = new JournalEntry
        {
            OrganizationId = orgId,
            EntryDate = DateTime.UtcNow.Date,
            Description = $"Payment for Vendor Bill {bill.BillNumber}",
            Status = "Posted",
            OwnerId = bill.OwnerId,
            Lines = new List<JournalEntryLine>
            {
                new() { LedgerAccountId = bill.PayableLedgerAccountId, Debit = total, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccountId = dto.PaymentLedgerAccountId, Debit = 0m, Credit = total, DisplayOrder = 2 }
            }
        };

        _journalRepo.Add(entry);
        await _journalRepo.SaveChangesAsync();

        entry.EntryNumber = $"JE-{entry.Id:D5}";
        _journalRepo.Update(entry);
        await _journalRepo.SaveChangesAsync();

        bill.Status = "Paid";
        bill.PaymentJournalEntryId = entry.Id;
        bill.UpdatedDate = DateTime.UtcNow;
        _repo.Update(bill);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, bill.Id) ?? bill;
        return ToDto(reloaded);
    }

    public async Task<VendorBillDto> CancelAsync(int orgId, int id)
    {
        var bill = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"VendorBill {id} not found");

        if (bill.Status != "Draft")
            throw new InvalidOperationException("Only draft vendor bills can be cancelled.");

        bill.Status = "Voided";
        bill.UpdatedDate = DateTime.UtcNow;
        _repo.Update(bill);
        await _repo.SaveChangesAsync();

        return ToDto(bill);
    }

    private static VendorBillDto ToDto(VendorBill b)
    {
        var lineDtos = b.Lines.OrderBy(l => l.DisplayOrder).Select(l => new VendorBillLineDto
        {
            Id = l.Id,
            LedgerAccountId = l.LedgerAccountId,
            LedgerAccountName = l.LedgerAccount?.Name ?? string.Empty,
            LedgerAccountCode = l.LedgerAccount?.Code ?? string.Empty,
            Description = l.Description,
            Amount = l.Amount,
            DisplayOrder = l.DisplayOrder
        }).ToList();

        return new VendorBillDto
        {
            Id = b.Id,
            BillNumber = b.BillNumber,
            VendorId = b.VendorId,
            VendorName = b.Vendor?.Name ?? string.Empty,
            PayableLedgerAccountId = b.PayableLedgerAccountId,
            PayableLedgerAccountName = b.PayableLedgerAccount?.Name ?? string.Empty,
            BillDate = b.BillDate,
            DueDate = b.DueDate,
            Status = b.Status,
            OwnerId = b.OwnerId,
            OwnerName = b.Owner?.Name ?? string.Empty,
            PostedJournalEntryId = b.PostedJournalEntryId,
            PaymentJournalEntryId = b.PaymentJournalEntryId,
            Lines = lineDtos,
            TotalAmount = lineDtos.Sum(l => l.Amount)
        };
    }
}
