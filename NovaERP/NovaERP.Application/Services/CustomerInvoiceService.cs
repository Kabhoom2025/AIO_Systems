using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class CustomerInvoiceService : ICustomerInvoiceService
{
    private readonly ICustomerInvoiceRepository _repo;
    private readonly IJournalEntryRepository _journalRepo;
    private readonly IValidator<CreateCustomerInvoiceDto> _createValidator;
    private readonly IValidator<UpdateCustomerInvoiceDto> _updateValidator;
    private readonly IValidator<ReceiveCustomerInvoicePaymentDto> _paymentValidator;

    public CustomerInvoiceService(ICustomerInvoiceRepository repo, IJournalEntryRepository journalRepo,
        IValidator<CreateCustomerInvoiceDto> createValidator, IValidator<UpdateCustomerInvoiceDto> updateValidator,
        IValidator<ReceiveCustomerInvoicePaymentDto> paymentValidator)
    {
        _repo = repo;
        _journalRepo = journalRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _paymentValidator = paymentValidator;
    }

    public async Task<List<CustomerInvoiceDto>> GetAllAsync(int orgId)
    {
        var invoices = await _repo.GetAllByOrgAsync(orgId);
        return invoices.Select(ToDto).ToList();
    }

    public async Task<CustomerInvoiceDto> GetByIdAsync(int orgId, int id)
    {
        var invoice = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"CustomerInvoice {id} not found");
        return ToDto(invoice);
    }

    public async Task<CustomerInvoiceDto> CreateAsync(int orgId, CreateCustomerInvoiceDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var invoice = new CustomerInvoice
        {
            OrganizationId = orgId,
            AccountId = dto.AccountId,
            ReceivableLedgerAccountId = dto.ReceivableLedgerAccountId,
            InvoiceDate = dto.InvoiceDate,
            DueDate = dto.DueDate,
            Status = "Draft",
            OwnerId = dto.OwnerId,
            Lines = dto.Lines.Select(l => new CustomerInvoiceLine
            {
                LedgerAccountId = l.LedgerAccountId,
                Description = l.Description,
                Amount = l.Amount,
                DisplayOrder = l.DisplayOrder
            }).ToList()
        };

        _repo.Add(invoice);
        await _repo.SaveChangesAsync();

        // InvoiceNumber depends on the generated Id, so it's set in a second save — same
        // scheme as VendorBill.BillNumber/JournalEntry.EntryNumber.
        invoice.InvoiceNumber = $"INV-{invoice.Id:D5}";
        _repo.Update(invoice);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, invoice.Id) ?? invoice;
        return ToDto(reloaded);
    }

    public async Task<CustomerInvoiceDto> UpdateAsync(int orgId, int id, UpdateCustomerInvoiceDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var invoice = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"CustomerInvoice {id} not found");

        if (invoice.Status != "Draft")
            throw new InvalidOperationException("Only draft customer invoices can be edited.");

        invoice.ReceivableLedgerAccountId = dto.ReceivableLedgerAccountId;
        invoice.InvoiceDate = dto.InvoiceDate;
        invoice.DueDate = dto.DueDate;
        invoice.OwnerId = dto.OwnerId;
        invoice.UpdatedDate = DateTime.UtcNow;

        // Lines are owned by the invoice and replaced wholesale on update.
        invoice.Lines.Clear();
        foreach (var l in dto.Lines)
        {
            invoice.Lines.Add(new CustomerInvoiceLine
            {
                LedgerAccountId = l.LedgerAccountId,
                Description = l.Description,
                Amount = l.Amount,
                DisplayOrder = l.DisplayOrder
            });
        }

        _repo.Update(invoice);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, invoice.Id) ?? invoice;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var invoice = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"CustomerInvoice {id} not found");

        if (invoice.Status != "Draft")
            throw new InvalidOperationException("Only draft customer invoices can be deleted.");

        _repo.Remove(invoice);
        await _repo.SaveChangesAsync();
    }

    public async Task<CustomerInvoiceDto> SendAsync(int orgId, int id)
    {
        var invoice = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"CustomerInvoice {id} not found");

        if (invoice.Status != "Draft")
            throw new InvalidOperationException("Only draft customer invoices can be sent.");

        var total = invoice.Lines.Sum(l => l.Amount);
        var entry = new JournalEntry
        {
            OrganizationId = orgId,
            EntryDate = DateTime.UtcNow.Date,
            Description = $"Customer Invoice {invoice.InvoiceNumber}",
            Status = "Posted",
            OwnerId = invoice.OwnerId,
            Lines = new List<JournalEntryLine>
            {
                new() { LedgerAccountId = invoice.ReceivableLedgerAccountId, Debit = total, Credit = 0m, DisplayOrder = 1 }
            }
        };
        var order = 2;
        foreach (var line in invoice.Lines)
        {
            entry.Lines.Add(new JournalEntryLine
            {
                LedgerAccountId = line.LedgerAccountId,
                Debit = 0m,
                Credit = line.Amount,
                DisplayOrder = order++
            });
        }

        _journalRepo.Add(entry);
        await _journalRepo.SaveChangesAsync();

        entry.EntryNumber = $"JE-{entry.Id:D5}";
        _journalRepo.Update(entry);
        await _journalRepo.SaveChangesAsync();

        invoice.Status = "Sent";
        invoice.PostedJournalEntryId = entry.Id;
        invoice.UpdatedDate = DateTime.UtcNow;
        _repo.Update(invoice);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, invoice.Id) ?? invoice;
        return ToDto(reloaded);
    }

    public async Task<CustomerInvoiceDto> ReceivePaymentAsync(int orgId, int id, ReceiveCustomerInvoicePaymentDto dto)
    {
        await _paymentValidator.ValidateAndThrowAsync(dto);

        var invoice = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"CustomerInvoice {id} not found");

        if (invoice.Status != "Sent")
            throw new InvalidOperationException("Only sent customer invoices can receive payment.");

        var total = invoice.Lines.Sum(l => l.Amount);
        var entry = new JournalEntry
        {
            OrganizationId = orgId,
            EntryDate = DateTime.UtcNow.Date,
            Description = $"Payment for Customer Invoice {invoice.InvoiceNumber}",
            Status = "Posted",
            OwnerId = invoice.OwnerId,
            Lines = new List<JournalEntryLine>
            {
                new() { LedgerAccountId = dto.PaymentLedgerAccountId, Debit = total, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccountId = invoice.ReceivableLedgerAccountId, Debit = 0m, Credit = total, DisplayOrder = 2 }
            }
        };

        _journalRepo.Add(entry);
        await _journalRepo.SaveChangesAsync();

        entry.EntryNumber = $"JE-{entry.Id:D5}";
        _journalRepo.Update(entry);
        await _journalRepo.SaveChangesAsync();

        invoice.Status = "Paid";
        invoice.PaymentJournalEntryId = entry.Id;
        invoice.UpdatedDate = DateTime.UtcNow;
        _repo.Update(invoice);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, invoice.Id) ?? invoice;
        return ToDto(reloaded);
    }

    public async Task<CustomerInvoiceDto> CancelAsync(int orgId, int id)
    {
        var invoice = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"CustomerInvoice {id} not found");

        if (invoice.Status != "Draft")
            throw new InvalidOperationException("Only draft customer invoices can be cancelled.");

        invoice.Status = "Voided";
        invoice.UpdatedDate = DateTime.UtcNow;
        _repo.Update(invoice);
        await _repo.SaveChangesAsync();

        return ToDto(invoice);
    }

    private static CustomerInvoiceDto ToDto(CustomerInvoice i)
    {
        var lineDtos = i.Lines.OrderBy(l => l.DisplayOrder).Select(l => new CustomerInvoiceLineDto
        {
            Id = l.Id,
            LedgerAccountId = l.LedgerAccountId,
            LedgerAccountName = l.LedgerAccount?.Name ?? string.Empty,
            LedgerAccountCode = l.LedgerAccount?.Code ?? string.Empty,
            Description = l.Description,
            Amount = l.Amount,
            DisplayOrder = l.DisplayOrder
        }).ToList();

        return new CustomerInvoiceDto
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            AccountId = i.AccountId,
            AccountName = i.Account?.Name ?? string.Empty,
            ReceivableLedgerAccountId = i.ReceivableLedgerAccountId,
            ReceivableLedgerAccountName = i.ReceivableLedgerAccount?.Name ?? string.Empty,
            InvoiceDate = i.InvoiceDate,
            DueDate = i.DueDate,
            Status = i.Status,
            OwnerId = i.OwnerId,
            OwnerName = i.Owner?.Name ?? string.Empty,
            PostedJournalEntryId = i.PostedJournalEntryId,
            PaymentJournalEntryId = i.PaymentJournalEntryId,
            Lines = lineDtos,
            TotalAmount = lineDtos.Sum(l => l.Amount)
        };
    }
}
