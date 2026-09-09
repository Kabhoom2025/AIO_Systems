using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class PosSaleService : IPosSaleService
{
    private readonly IPosSaleRepository _repo;
    private readonly IStockMovementRepository _movementRepo;
    private readonly IJournalEntryRepository _journalRepo;
    private readonly IValidator<CreatePosSaleDto> _createValidator;
    private readonly IValidator<UpdatePosSaleDto> _updateValidator;
    private readonly IValidator<CompletePosSaleDto> _completeValidator;

    public PosSaleService(IPosSaleRepository repo, IStockMovementRepository movementRepo,
        IJournalEntryRepository journalRepo, IValidator<CreatePosSaleDto> createValidator,
        IValidator<UpdatePosSaleDto> updateValidator, IValidator<CompletePosSaleDto> completeValidator)
    {
        _repo = repo;
        _movementRepo = movementRepo;
        _journalRepo = journalRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _completeValidator = completeValidator;
    }

    public async Task<List<PosSaleDto>> GetAllAsync(int orgId)
    {
        var sales = await _repo.GetAllByOrgAsync(orgId);
        return sales.Select(ToDto).ToList();
    }

    public async Task<PosSaleDto> GetByIdAsync(int orgId, int id)
    {
        var sale = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PosSale {id} not found");
        return ToDto(sale);
    }

    public async Task<PosSaleDto> CreateAsync(int orgId, CreatePosSaleDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var sale = new PosSale
        {
            OrganizationId = orgId,
            WarehouseId = dto.WarehouseId,
            CustomerAccountId = dto.CustomerAccountId,
            SaleDate = dto.SaleDate,
            RevenueLedgerAccountId = dto.RevenueLedgerAccountId,
            OwnerId = dto.OwnerId,
            Status = "Draft",
            Lines = dto.Lines.Select(l => new PosSaleLine
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        _repo.Add(sale);
        await _repo.SaveChangesAsync();

        // SaleNumber depends on the generated Id, so it's set in a second save — same scheme
        // as CustomerInvoice.InvoiceNumber/VendorBill.BillNumber.
        sale.SaleNumber = $"POS-{sale.Id:D5}";
        _repo.Update(sale);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, sale.Id) ?? sale;
        return ToDto(reloaded);
    }

    public async Task<PosSaleDto> UpdateAsync(int orgId, int id, UpdatePosSaleDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var sale = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PosSale {id} not found");

        if (sale.Status != "Draft")
            throw new InvalidOperationException("Only draft POS sales can be edited.");

        sale.WarehouseId = dto.WarehouseId;
        sale.CustomerAccountId = dto.CustomerAccountId;
        sale.SaleDate = dto.SaleDate;
        sale.RevenueLedgerAccountId = dto.RevenueLedgerAccountId;
        sale.OwnerId = dto.OwnerId;
        sale.UpdatedDate = DateTime.UtcNow;

        // Lines are owned by the sale and replaced wholesale on update.
        sale.Lines.Clear();
        foreach (var l in dto.Lines)
        {
            sale.Lines.Add(new PosSaleLine
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            });
        }

        _repo.Update(sale);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, sale.Id) ?? sale;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var sale = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PosSale {id} not found");

        if (sale.Status != "Draft")
            throw new InvalidOperationException("Only draft POS sales can be deleted.");

        _repo.Remove(sale);
        await _repo.SaveChangesAsync();
    }

    public async Task<PosSaleDto> CompleteAsync(int orgId, int id, CompletePosSaleDto dto)
    {
        await _completeValidator.ValidateAndThrowAsync(dto);

        var sale = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PosSale {id} not found");

        if (sale.Status != "Draft")
            throw new InvalidOperationException("Only draft POS sales can be completed.");

        foreach (var line in sale.Lines)
        {
            var onHand = await _movementRepo.GetOnHandAtWarehouseAsync(orgId, line.ProductId, sale.WarehouseId);
            if (onHand < line.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock of product {line.ProductId} at warehouse: on hand {onHand}, required {line.Quantity}");
        }

        var total = sale.Lines.Sum(l => l.Quantity * l.UnitPrice);
        var entry = new JournalEntry
        {
            OrganizationId = orgId,
            EntryDate = DateTime.UtcNow.Date,
            Description = $"POS Sale {sale.SaleNumber}",
            Status = "Posted",
            OwnerId = sale.OwnerId,
            Lines = new List<JournalEntryLine>
            {
                new() { LedgerAccountId = dto.PaymentLedgerAccountId, Debit = total, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccountId = sale.RevenueLedgerAccountId, Debit = 0m, Credit = total, DisplayOrder = 2 }
            }
        };

        _journalRepo.Add(entry);
        await _journalRepo.SaveChangesAsync();

        entry.EntryNumber = $"JE-{entry.Id:D5}";
        _journalRepo.Update(entry);
        await _journalRepo.SaveChangesAsync();

        foreach (var line in sale.Lines)
        {
            _movementRepo.Add(new StockMovement
            {
                OrganizationId = orgId,
                ProductId = line.ProductId,
                WarehouseId = sale.WarehouseId,
                MovementType = "Issue",
                Quantity = -line.Quantity,
                EntityType = "PosSale",
                EntityId = sale.Id,
                MovementDate = DateTime.UtcNow
            });
        }
        await _movementRepo.SaveChangesAsync();

        sale.PaymentLedgerAccountId = dto.PaymentLedgerAccountId;
        sale.PostedJournalEntryId = entry.Id;
        sale.Status = "Completed";
        sale.UpdatedDate = DateTime.UtcNow;
        _repo.Update(sale);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, sale.Id) ?? sale;
        return ToDto(reloaded);
    }

    public async Task<PosSaleDto> RefundAsync(int orgId, int id)
    {
        var sale = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PosSale {id} not found");

        if (sale.Status != "Completed")
            throw new InvalidOperationException("Only completed POS sales can be refunded.");

        var total = sale.Lines.Sum(l => l.Quantity * l.UnitPrice);
        var entry = new JournalEntry
        {
            OrganizationId = orgId,
            EntryDate = DateTime.UtcNow.Date,
            Description = $"Refund for POS Sale {sale.SaleNumber}",
            Status = "Posted",
            OwnerId = sale.OwnerId,
            Lines = new List<JournalEntryLine>
            {
                new() { LedgerAccountId = sale.RevenueLedgerAccountId, Debit = total, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccountId = sale.PaymentLedgerAccountId!.Value, Debit = 0m, Credit = total, DisplayOrder = 2 }
            }
        };

        _journalRepo.Add(entry);
        await _journalRepo.SaveChangesAsync();

        entry.EntryNumber = $"JE-{entry.Id:D5}";
        _journalRepo.Update(entry);
        await _journalRepo.SaveChangesAsync();

        foreach (var line in sale.Lines)
        {
            _movementRepo.Add(new StockMovement
            {
                OrganizationId = orgId,
                ProductId = line.ProductId,
                WarehouseId = sale.WarehouseId,
                MovementType = "Receipt",
                Quantity = line.Quantity,
                EntityType = "PosSale",
                EntityId = sale.Id,
                MovementDate = DateTime.UtcNow
            });
        }
        await _movementRepo.SaveChangesAsync();

        sale.Status = "Refunded";
        sale.UpdatedDate = DateTime.UtcNow;
        _repo.Update(sale);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, sale.Id) ?? sale;
        return ToDto(reloaded);
    }

    public async Task<PosSaleDto> CancelAsync(int orgId, int id)
    {
        var sale = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PosSale {id} not found");

        if (sale.Status != "Draft")
            throw new InvalidOperationException("Only draft POS sales can be cancelled.");

        sale.Status = "Cancelled";
        sale.UpdatedDate = DateTime.UtcNow;
        _repo.Update(sale);
        await _repo.SaveChangesAsync();

        return ToDto(sale);
    }

    private static PosSaleDto ToDto(PosSale s)
    {
        var lineDtos = s.Lines.Select(l => new PosSaleLineDto
        {
            Id = l.Id,
            ProductId = l.ProductId,
            ProductName = l.Product?.Name ?? string.Empty,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            LineTotal = l.Quantity * l.UnitPrice
        }).ToList();

        return new PosSaleDto
        {
            Id = s.Id,
            SaleNumber = s.SaleNumber,
            WarehouseId = s.WarehouseId,
            WarehouseName = s.Warehouse?.Name ?? string.Empty,
            CustomerAccountId = s.CustomerAccountId,
            CustomerAccountName = s.CustomerAccount?.Name,
            SaleDate = s.SaleDate,
            RevenueLedgerAccountId = s.RevenueLedgerAccountId,
            RevenueLedgerAccountName = s.RevenueLedgerAccount?.Name ?? string.Empty,
            PaymentLedgerAccountId = s.PaymentLedgerAccountId,
            PaymentLedgerAccountName = s.PaymentLedgerAccount?.Name,
            Status = s.Status,
            OwnerId = s.OwnerId,
            OwnerName = s.Owner?.Name ?? string.Empty,
            PostedJournalEntryId = s.PostedJournalEntryId,
            Lines = lineDtos,
            TotalAmount = lineDtos.Sum(l => l.LineTotal)
        };
    }
}
