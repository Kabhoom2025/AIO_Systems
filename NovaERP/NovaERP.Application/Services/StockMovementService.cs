using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class StockMovementService : IStockMovementService
{
    private readonly IStockMovementRepository _repo;
    private readonly IProductRepository _productRepo;
    private readonly IWarehouseRepository _warehouseRepo;
    private readonly IValidator<CreateStockMovementDto> _createValidator;

    public StockMovementService(IStockMovementRepository repo, IProductRepository productRepo,
        IWarehouseRepository warehouseRepo, IValidator<CreateStockMovementDto> createValidator)
    {
        _repo = repo;
        _productRepo = productRepo;
        _warehouseRepo = warehouseRepo;
        _createValidator = createValidator;
    }

    public async Task<List<StockMovementDto>> GetAllAsync(int orgId, int? productId)
    {
        var movements = await _repo.GetAllByOrgAsync(orgId, productId);
        return movements.Select(ToDto).ToList();
    }

    public async Task<StockMovementDto> CreateAsync(int orgId, CreateStockMovementDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var product = await _productRepo.GetByIdAsync(orgId, dto.ProductId)
            ?? throw new KeyNotFoundException($"Product {dto.ProductId} not found");

        var warehouse = dto.WarehouseId.HasValue
            ? await _warehouseRepo.GetByIdAsync(orgId, dto.WarehouseId.Value)
                ?? throw new KeyNotFoundException($"Warehouse {dto.WarehouseId} not found")
            : null;

        var movement = new StockMovement
        {
            OrganizationId = orgId,
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            MovementType = dto.MovementType,
            Quantity = dto.Quantity,
            Notes = dto.Notes,
            MovementDate = DateTime.UtcNow
        };

        _repo.Add(movement);
        await _repo.SaveChangesAsync();

        movement.Product = product;
        movement.Warehouse = warehouse;
        return ToDto(movement);
    }

    public Task<decimal> GetOnHandAtWarehouseAsync(int orgId, int productId, int warehouseId) =>
        _repo.GetOnHandAtWarehouseAsync(orgId, productId, warehouseId);

    private static StockMovementDto ToDto(StockMovement m) => new()
    {
        Id = m.Id,
        ProductId = m.ProductId,
        ProductName = m.Product?.Name ?? string.Empty,
        ProductSku = m.Product?.Sku ?? string.Empty,
        WarehouseId = m.WarehouseId,
        WarehouseName = m.Warehouse?.Name,
        MovementType = m.MovementType,
        Quantity = m.Quantity,
        EntityType = m.EntityType,
        EntityId = m.EntityId,
        MovementDate = m.MovementDate,
        Notes = m.Notes
    };
}
