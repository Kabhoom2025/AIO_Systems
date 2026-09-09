using AutoMapper;
using FoodOrder.Application.Common;
using FoodOrder.Application.DTOs;
using FoodOrder.Application.Interfaces;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repo;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly IMapper _mapper;

    public InventoryService(IInventoryRepository repo, IBranchRepository branchRepository, ICurrentUserContext currentUser, IMapper mapper)
    {
        _repo = repo;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<InventoryItemDTO>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return _mapper.Map<IReadOnlyList<InventoryItemDTO>>(items);
    }

    public async Task<InventoryItemDTO> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException("InventoryItem", id);
        return _mapper.Map<InventoryItemDTO>(item);
    }

    public async Task<IReadOnlyList<InventoryItemDTO>> GetLowStockAsync()
    {
        var items = await _repo.GetLowStockAsync();
        return _mapper.Map<IReadOnlyList<InventoryItemDTO>>(items);
    }

    public async Task<IReadOnlyList<InventoryItemDTO>> GetExpiringAsync(int withinDays)
    {
        var items = await _repo.GetExpiringAsync(withinDays);
        return _mapper.Map<IReadOnlyList<InventoryItemDTO>>(items);
    }

    public async Task<InventoryItemDTO> CreateAsync(CreateInventoryItemRequest dto)
    {
        var item = _mapper.Map<Domain.Entities.InventoryItem>(dto);
        item.BranchId = await TenantResolution.ResolveWriteBranchIdAsync(_currentUser, _branchRepository);
        item.LastUpdated = DateTime.UtcNow;
        await _repo.AddAsync(item);
        await _repo.SaveChangesAsync();

        if (item.CurrentStock > 0)
        {
            var tx = new StockTransaction
            {
                BranchId = item.BranchId,
                InventoryItemId = item.Id,
                TransactionType = StockTransactionType.Opening,
                Quantity = item.CurrentStock,
                StockBefore = 0,
                StockAfter = item.CurrentStock,
                ExpiryDate = item.ExpiryDate,
                Notes = "Opening stock",
                CreatedAt = DateTime.UtcNow,
            };
            await _repo.AddTransactionAsync(tx);
            await _repo.SaveChangesAsync();
        }

        return _mapper.Map<InventoryItemDTO>(item);
    }

    public async Task<InventoryItemDTO> UpdateAsync(int id, UpdateInventoryItemRequest dto)
    {
        var item = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException("InventoryItem", id);
        _mapper.Map(dto, item);
        item.LastUpdated = DateTime.UtcNow;
        await _repo.UpdateAsync(item);
        await _repo.SaveChangesAsync();
        return _mapper.Map<InventoryItemDTO>(item);
    }

    public async Task<InventoryItemDTO> AdjustStockAsync(int id, StockAdjustmentRequest dto)
    {
        var item = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException("InventoryItem", id);

        var before = item.CurrentStock;
        item.CurrentStock += dto.Quantity;
        if (item.CurrentStock < 0) item.CurrentStock = 0;
        item.LastUpdated = DateTime.UtcNow;

        await _repo.UpdateAsync(item);

        var tx = new StockTransaction
        {
            BranchId = item.BranchId,
            InventoryItemId = item.Id,
            TransactionType = StockTransactionType.Adjustment,
            Quantity = dto.Quantity,
            StockBefore = before,
            StockAfter = item.CurrentStock,
            Notes = dto.Note,
            CreatedAt = DateTime.UtcNow,
        };
        await _repo.AddTransactionAsync(tx);
        await _repo.SaveChangesAsync();

        return _mapper.Map<InventoryItemDTO>(item);
    }

    public async Task<InventoryItemDTO> PurchaseStockAsync(int id, PurchaseStockRequest dto)
    {
        if (dto.Quantity <= 0)
            throw new ArgumentException("Purchase quantity must be greater than zero.");

        var item = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException("InventoryItem", id);

        var before = item.CurrentStock;
        item.CurrentStock += dto.Quantity;
        item.LastUpdated = DateTime.UtcNow;

        // Update item-level expiry if this purchase has one
        if (dto.ExpiryDate.HasValue)
            item.ExpiryDate = dto.ExpiryDate;

        await _repo.UpdateAsync(item);

        var tx = new StockTransaction
        {
            BranchId = item.BranchId,
            InventoryItemId = item.Id,
            TransactionType = StockTransactionType.Purchase,
            Quantity = dto.Quantity,
            StockBefore = before,
            StockAfter = item.CurrentStock,
            BatchNumber = dto.BatchNumber,
            ExpiryDate = dto.ExpiryDate.HasValue
                ? DateTime.SpecifyKind(dto.ExpiryDate.Value, DateTimeKind.Utc)
                : null,
            Supplier = dto.Supplier,
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
        };
        await _repo.AddTransactionAsync(tx);
        await _repo.SaveChangesAsync();

        return _mapper.Map<InventoryItemDTO>(item);
    }

    public async Task<InventoryItemDTO> WasteStockAsync(int id, WasteStockRequest dto)
    {
        if (dto.Quantity <= 0)
            throw new ArgumentException("Waste quantity must be greater than zero.");

        var item = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException("InventoryItem", id);

        var before = item.CurrentStock;
        item.CurrentStock = Math.Max(0, item.CurrentStock - dto.Quantity);
        item.LastUpdated = DateTime.UtcNow;

        await _repo.UpdateAsync(item);

        var tx = new StockTransaction
        {
            BranchId = item.BranchId,
            InventoryItemId = item.Id,
            TransactionType = StockTransactionType.Waste,
            Quantity = -dto.Quantity,
            StockBefore = before,
            StockAfter = item.CurrentStock,
            BatchNumber = dto.BatchNumber,
            Notes = string.IsNullOrWhiteSpace(dto.WasteReason) ? dto.Notes : $"{dto.WasteReason}{(dto.Notes != null ? " — " + dto.Notes : "")}",
            CreatedAt = DateTime.UtcNow,
        };
        await _repo.AddTransactionAsync(tx);
        await _repo.SaveChangesAsync();

        return _mapper.Map<InventoryItemDTO>(item);
    }

    public async Task<(InventoryItemDTO source, InventoryItemDTO target)> TransferStockAsync(int id, TransferStockRequest dto)
    {
        if (dto.Quantity <= 0)
            throw new ArgumentException("Transfer quantity must be greater than zero.");

        if (id == dto.TargetInventoryItemId)
            throw new ArgumentException("Source and target items must be different.");

        var source = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException("InventoryItem (source)", id);

        var target = await _repo.GetByIdAsync(dto.TargetInventoryItemId)
            ?? throw new NotFoundException("InventoryItem (target)", dto.TargetInventoryItemId);

        var srcBefore = source.CurrentStock;
        source.CurrentStock = Math.Max(0, source.CurrentStock - dto.Quantity);
        source.LastUpdated = DateTime.UtcNow;

        var tgtBefore = target.CurrentStock;
        target.CurrentStock += dto.Quantity;
        target.LastUpdated = DateTime.UtcNow;

        await _repo.UpdateAsync(source);
        await _repo.UpdateAsync(target);

        var srcTx = new StockTransaction
        {
            BranchId = source.BranchId,
            InventoryItemId = source.Id,
            TransactionType = StockTransactionType.Transfer,
            Quantity = -dto.Quantity,
            StockBefore = srcBefore,
            StockAfter = source.CurrentStock,
            Notes = $"Transfer to: {target.Name}{(dto.Notes != null ? " — " + dto.Notes : "")}",
            CreatedAt = DateTime.UtcNow,
        };
        var tgtTx = new StockTransaction
        {
            BranchId = target.BranchId,
            InventoryItemId = target.Id,
            TransactionType = StockTransactionType.Transfer,
            Quantity = dto.Quantity,
            StockBefore = tgtBefore,
            StockAfter = target.CurrentStock,
            Notes = $"Transfer from: {source.Name}{(dto.Notes != null ? " — " + dto.Notes : "")}",
            CreatedAt = DateTime.UtcNow,
        };

        await _repo.AddTransactionAsync(srcTx);
        await _repo.AddTransactionAsync(tgtTx);
        await _repo.SaveChangesAsync();

        // Link the two transfer transactions together
        srcTx.RelatedTransactionId = tgtTx.Id;
        tgtTx.RelatedTransactionId = srcTx.Id;
        await _repo.SaveChangesAsync();

        return (_mapper.Map<InventoryItemDTO>(source), _mapper.Map<InventoryItemDTO>(target));
    }

    public async Task<IReadOnlyList<StockTransactionDTO>> GetTransactionsAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("InventoryItem", id);
        var txs = await _repo.GetTransactionsAsync(id);
        return txs.Select(t => new StockTransactionDTO
        {
            Id = t.Id,
            TransactionType = t.TransactionType.ToString(),
            Quantity = t.Quantity,
            StockBefore = t.StockBefore,
            StockAfter = t.StockAfter,
            BatchNumber = t.BatchNumber,
            ExpiryDate = t.ExpiryDate,
            Supplier = t.Supplier,
            ReferenceNumber = t.ReferenceNumber,
            Notes = t.Notes,
            RelatedTransactionId = t.RelatedTransactionId,
            CreatedAt = t.CreatedAt,
        }).ToList();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException("InventoryItem", id);
        await _repo.DeleteAsync(item.Id);
        await _repo.SaveChangesAsync();
    }

    public async Task<InventoryItemDTO?> GetByBarcodeAsync(string barcode)
    {
        var item = await _repo.GetByBarcodeAsync(barcode.Trim());
        return item is null ? null : _mapper.Map<InventoryItemDTO>(item);
    }
}
