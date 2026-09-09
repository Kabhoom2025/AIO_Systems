using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class ProductionOrderService : IProductionOrderService
{
    private readonly IProductionOrderRepository _repo;
    private readonly IBillOfMaterialRepository _bomRepo;
    private readonly IStockMovementRepository _movementRepo;
    private readonly IValidator<CreateProductionOrderDto> _createValidator;
    private readonly IValidator<UpdateProductionOrderDto> _updateValidator;

    public ProductionOrderService(IProductionOrderRepository repo, IBillOfMaterialRepository bomRepo,
        IStockMovementRepository movementRepo,
        IValidator<CreateProductionOrderDto> createValidator, IValidator<UpdateProductionOrderDto> updateValidator)
    {
        _repo = repo;
        _bomRepo = bomRepo;
        _movementRepo = movementRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<ProductionOrderDto>> GetAllAsync(int orgId)
    {
        var orders = await _repo.GetAllByOrgAsync(orgId);
        return orders.Select(ToDto).ToList();
    }

    public async Task<ProductionOrderDto> GetByIdAsync(int orgId, int id)
    {
        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ProductionOrder {id} not found");
        return ToDto(order);
    }

    public async Task<ProductionOrderDto> CreateAsync(int orgId, CreateProductionOrderDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var order = new ProductionOrder
        {
            OrganizationId = orgId,
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            Quantity = dto.Quantity,
            Status = "Draft",
            OrderDate = dto.OrderDate,
            OwnerId = dto.OwnerId
        };

        _repo.Add(order);
        await _repo.SaveChangesAsync();

        // MoNumber depends on the generated Id, so it's set in a second save — same scheme
        // as SalesOrder.OrderNumber/PurchaseOrder.PoNumber.
        order.MoNumber = $"MO-{order.Id:D5}";
        _repo.Update(order);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, order.Id) ?? order;
        return ToDto(reloaded);
    }

    public async Task<ProductionOrderDto> UpdateAsync(int orgId, int id, UpdateProductionOrderDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ProductionOrder {id} not found");

        if (order.Status != "Draft")
            throw new InvalidOperationException("Only draft production orders can be edited.");

        order.WarehouseId = dto.WarehouseId;
        order.Quantity = dto.Quantity;
        order.OrderDate = dto.OrderDate;
        order.OwnerId = dto.OwnerId;
        order.UpdatedDate = DateTime.UtcNow;

        _repo.Update(order);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, order.Id) ?? order;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ProductionOrder {id} not found");

        if (order.Status != "Draft")
            throw new InvalidOperationException("Only draft production orders can be deleted.");

        _repo.Remove(order);
        await _repo.SaveChangesAsync();
    }

    public async Task<ProductionOrderDto> CompleteAsync(int orgId, int id)
    {
        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ProductionOrder {id} not found");

        if (order.Status != "Draft")
            throw new InvalidOperationException("Only draft production orders can be completed.");

        var bom = await _bomRepo.GetByProductIdAsync(orgId, order.ProductId)
            ?? throw new InvalidOperationException($"No bill of materials exists for {order.Product?.Name ?? "this product"}.");

        // All components must have enough on-hand at the order's warehouse before anything
        // is committed — same "reject up front, don't partially commit" reasoning as
        // StockTransferService's source on-hand check, just generalized to N components.
        foreach (var component in bom.Components)
        {
            var required = component.Quantity * order.Quantity;
            var onHand = await _movementRepo.GetOnHandAtWarehouseAsync(orgId, component.ComponentProductId, order.WarehouseId);
            if (onHand < required)
                throw new InvalidOperationException(
                    $"Insufficient stock of {component.ComponentProduct?.Name} at warehouse: on hand {onHand}, required {required}");
        }

        order.Status = "Completed";
        order.UpdatedDate = DateTime.UtcNow;
        _repo.Update(order);
        await _repo.SaveChangesAsync();

        foreach (var component in bom.Components)
        {
            _movementRepo.Add(new StockMovement
            {
                OrganizationId = orgId,
                ProductId = component.ComponentProductId,
                WarehouseId = order.WarehouseId,
                MovementType = "Issue",
                Quantity = -(component.Quantity * order.Quantity),
                EntityType = "ProductionOrder",
                EntityId = order.Id,
                MovementDate = DateTime.UtcNow
            });
        }
        _movementRepo.Add(new StockMovement
        {
            OrganizationId = orgId,
            ProductId = order.ProductId,
            WarehouseId = order.WarehouseId,
            MovementType = "Receipt",
            Quantity = order.Quantity,
            EntityType = "ProductionOrder",
            EntityId = order.Id,
            MovementDate = DateTime.UtcNow
        });
        await _movementRepo.SaveChangesAsync();

        return ToDto(order);
    }

    public async Task<ProductionOrderDto> CancelAsync(int orgId, int id)
    {
        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ProductionOrder {id} not found");

        if (order.Status != "Draft")
            throw new InvalidOperationException("Only draft production orders can be cancelled.");

        order.Status = "Cancelled";
        order.UpdatedDate = DateTime.UtcNow;
        _repo.Update(order);
        await _repo.SaveChangesAsync();

        return ToDto(order);
    }

    private static ProductionOrderDto ToDto(ProductionOrder o) => new()
    {
        Id = o.Id,
        MoNumber = o.MoNumber,
        ProductId = o.ProductId,
        ProductName = o.Product?.Name ?? string.Empty,
        WarehouseId = o.WarehouseId,
        WarehouseName = o.Warehouse?.Name ?? string.Empty,
        Quantity = o.Quantity,
        Status = o.Status,
        OrderDate = o.OrderDate,
        OwnerId = o.OwnerId,
        OwnerName = o.Owner?.Name ?? string.Empty
    };
}
