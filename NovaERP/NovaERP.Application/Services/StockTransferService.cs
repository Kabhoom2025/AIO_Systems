using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class StockTransferService : IStockTransferService
{
    private readonly IStockTransferRepository _repo;
    private readonly IProductRepository _productRepo;
    private readonly IWarehouseRepository _warehouseRepo;
    private readonly IStockMovementRepository _movementRepo;
    private readonly IValidator<CreateStockTransferDto> _createValidator;

    public StockTransferService(IStockTransferRepository repo, IProductRepository productRepo,
        IWarehouseRepository warehouseRepo, IStockMovementRepository movementRepo,
        IValidator<CreateStockTransferDto> createValidator)
    {
        _repo = repo;
        _productRepo = productRepo;
        _warehouseRepo = warehouseRepo;
        _movementRepo = movementRepo;
        _createValidator = createValidator;
    }

    public async Task<List<StockTransferDto>> GetAllAsync(int orgId)
    {
        var transfers = await _repo.GetAllByOrgAsync(orgId);
        return transfers.Select(ToDto).ToList();
    }

    public async Task<StockTransferDto> CreateAsync(int orgId, CreateStockTransferDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var product = await _productRepo.GetByIdAsync(orgId, dto.ProductId)
            ?? throw new KeyNotFoundException($"Product {dto.ProductId} not found");
        var fromWarehouse = await _warehouseRepo.GetByIdAsync(orgId, dto.FromWarehouseId)
            ?? throw new KeyNotFoundException($"Warehouse {dto.FromWarehouseId} not found");
        var toWarehouse = await _warehouseRepo.GetByIdAsync(orgId, dto.ToWarehouseId)
            ?? throw new KeyNotFoundException($"Warehouse {dto.ToWarehouseId} not found");

        var onHandAtSource = await _movementRepo.GetOnHandAtWarehouseAsync(orgId, dto.ProductId, dto.FromWarehouseId);
        if (onHandAtSource < dto.Quantity)
            throw new InvalidOperationException(
                $"Insufficient stock at {fromWarehouse.Name}: on hand {onHandAtSource}, requested {dto.Quantity}");

        var transfer = new StockTransfer
        {
            OrganizationId = orgId,
            ProductId = dto.ProductId,
            FromWarehouseId = dto.FromWarehouseId,
            ToWarehouseId = dto.ToWarehouseId,
            Quantity = dto.Quantity,
            Notes = dto.Notes,
            TransferDate = DateTime.UtcNow
        };

        _repo.Add(transfer);
        await _repo.SaveChangesAsync();

        _movementRepo.Add(new StockMovement
        {
            OrganizationId = orgId,
            ProductId = dto.ProductId,
            WarehouseId = dto.FromWarehouseId,
            MovementType = "Issue",
            Quantity = -dto.Quantity,
            EntityType = "StockTransfer",
            EntityId = transfer.Id,
            MovementDate = transfer.TransferDate
        });
        _movementRepo.Add(new StockMovement
        {
            OrganizationId = orgId,
            ProductId = dto.ProductId,
            WarehouseId = dto.ToWarehouseId,
            MovementType = "Receipt",
            Quantity = dto.Quantity,
            EntityType = "StockTransfer",
            EntityId = transfer.Id,
            MovementDate = transfer.TransferDate
        });
        await _movementRepo.SaveChangesAsync();

        transfer.Product = product;
        transfer.FromWarehouse = fromWarehouse;
        transfer.ToWarehouse = toWarehouse;
        return ToDto(transfer);
    }

    private static StockTransferDto ToDto(StockTransfer t) => new()
    {
        Id = t.Id,
        ProductId = t.ProductId,
        ProductName = t.Product?.Name ?? string.Empty,
        ProductSku = t.Product?.Sku ?? string.Empty,
        FromWarehouseId = t.FromWarehouseId,
        FromWarehouseName = t.FromWarehouse?.Name ?? string.Empty,
        ToWarehouseId = t.ToWarehouseId,
        ToWarehouseName = t.ToWarehouse?.Name ?? string.Empty,
        Quantity = t.Quantity,
        TransferDate = t.TransferDate,
        Notes = t.Notes
    };
}
