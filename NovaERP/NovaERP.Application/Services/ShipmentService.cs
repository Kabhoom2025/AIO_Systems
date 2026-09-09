using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _repo;
    private readonly ISalesOrderRepository _salesOrderRepo;
    private readonly IStockMovementRepository _movementRepo;
    private readonly IValidator<CreateShipmentDto> _createValidator;
    private readonly IValidator<UpdateShipmentDto> _updateValidator;

    public ShipmentService(IShipmentRepository repo, ISalesOrderRepository salesOrderRepo,
        IStockMovementRepository movementRepo,
        IValidator<CreateShipmentDto> createValidator, IValidator<UpdateShipmentDto> updateValidator)
    {
        _repo = repo;
        _salesOrderRepo = salesOrderRepo;
        _movementRepo = movementRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<ShipmentDto>> GetAllAsync(int orgId)
    {
        var shipments = await _repo.GetAllByOrgAsync(orgId);
        return shipments.Select(ToDto).ToList();
    }

    public async Task<ShipmentDto> GetByIdAsync(int orgId, int id)
    {
        var shipment = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Shipment {id} not found");
        return ToDto(shipment);
    }

    public async Task<ShipmentDto> CreateAsync(int orgId, CreateShipmentDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        SalesOrder? salesOrder = null;
        if (dto.SourceType == "SalesOrder")
        {
            salesOrder = await _salesOrderRepo.GetByIdAsync(orgId, dto.SalesOrderId!.Value)
                ?? throw new KeyNotFoundException($"SalesOrder {dto.SalesOrderId} not found");
            if (salesOrder.Status != "Confirmed")
                throw new InvalidOperationException("Only confirmed sales orders can be shipped.");
        }

        var shipment = new Shipment
        {
            OrganizationId = orgId,
            WarehouseId = dto.WarehouseId,
            SourceType = dto.SourceType,
            SalesOrderId = dto.SalesOrderId,
            DestinationWarehouseId = dto.DestinationWarehouseId,
            ShipToName = dto.ShipToName,
            ShipToContactName = dto.ShipToContactName,
            ShipToEmail = dto.ShipToEmail,
            ShipToAddressLine1 = dto.ShipToAddressLine1,
            ShipToAddressLine2 = dto.ShipToAddressLine2,
            ShipToCity = dto.ShipToCity,
            ShipToState = dto.ShipToState,
            ShipToPostalCode = dto.ShipToPostalCode,
            ShipToCountry = dto.ShipToCountry,
            ShipToPhone = dto.ShipToPhone,
            ShipToTaxType = dto.ShipToTaxType,
            ShipToTaxCountry = dto.ShipToTaxCountry,
            ShipToTaxId = dto.ShipToTaxId,
            ShipFromName = dto.ShipFromName,
            ShipFromContactName = dto.ShipFromContactName,
            ShipFromEmail = dto.ShipFromEmail,
            ShipFromAddressLine1 = dto.ShipFromAddressLine1,
            ShipFromAddressLine2 = dto.ShipFromAddressLine2,
            ShipFromCity = dto.ShipFromCity,
            ShipFromState = dto.ShipFromState,
            ShipFromPostalCode = dto.ShipFromPostalCode,
            ShipFromCountry = dto.ShipFromCountry,
            ShipFromPhone = dto.ShipFromPhone,
            ShipDate = dto.ShipDate,
            Carrier = dto.Carrier,
            TrackingNumber = dto.TrackingNumber,
            IsBlindShipment = dto.IsBlindShipment,
            Status = "Open",
            OwnerId = dto.OwnerId,
            Lines = dto.Lines.Select(l => new ShipmentLine
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                SalesOrderLineId = l.SalesOrderLineId,
                DisplayOrder = l.DisplayOrder
            }).ToList(),
            Packages = dto.Packages.Select(p => new ShipmentPackage
            {
                PackageNumber = p.PackageNumber,
                WeightKg = p.WeightKg,
                LengthCm = p.LengthCm,
                WidthCm = p.WidthCm,
                HeightCm = p.HeightCm,
                TrackingNumber = p.TrackingNumber,
                Items = p.Items.Select(i => new ShipmentPackageItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            }).ToList()
        };

        _repo.Add(shipment);
        await _repo.SaveChangesAsync();

        // ShipmentNumber depends on the generated Id, so it's set in a second save — same
        // scheme as SalesOrder.OrderNumber/ProductionOrder.MoNumber.
        shipment.ShipmentNumber = $"SHIP-{shipment.Id:D5}";
        _repo.Update(shipment);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, shipment.Id) ?? shipment;
        return ToDto(reloaded);
    }

    public async Task<ShipmentDto> UpdateAsync(int orgId, int id, UpdateShipmentDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var shipment = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Shipment {id} not found");

        if (shipment.Status != "Open")
            throw new InvalidOperationException("Only open shipments can be edited.");

        shipment.WarehouseId = dto.WarehouseId;
        shipment.ShipToName = dto.ShipToName;
        shipment.ShipToContactName = dto.ShipToContactName;
        shipment.ShipToEmail = dto.ShipToEmail;
        shipment.ShipToAddressLine1 = dto.ShipToAddressLine1;
        shipment.ShipToAddressLine2 = dto.ShipToAddressLine2;
        shipment.ShipToCity = dto.ShipToCity;
        shipment.ShipToState = dto.ShipToState;
        shipment.ShipToPostalCode = dto.ShipToPostalCode;
        shipment.ShipToCountry = dto.ShipToCountry;
        shipment.ShipToPhone = dto.ShipToPhone;
        shipment.ShipToTaxType = dto.ShipToTaxType;
        shipment.ShipToTaxCountry = dto.ShipToTaxCountry;
        shipment.ShipToTaxId = dto.ShipToTaxId;
        shipment.ShipFromName = dto.ShipFromName;
        shipment.ShipFromContactName = dto.ShipFromContactName;
        shipment.ShipFromEmail = dto.ShipFromEmail;
        shipment.ShipFromAddressLine1 = dto.ShipFromAddressLine1;
        shipment.ShipFromAddressLine2 = dto.ShipFromAddressLine2;
        shipment.ShipFromCity = dto.ShipFromCity;
        shipment.ShipFromState = dto.ShipFromState;
        shipment.ShipFromPostalCode = dto.ShipFromPostalCode;
        shipment.ShipFromCountry = dto.ShipFromCountry;
        shipment.ShipFromPhone = dto.ShipFromPhone;
        shipment.ShipDate = dto.ShipDate;
        shipment.Carrier = dto.Carrier;
        shipment.TrackingNumber = dto.TrackingNumber;
        shipment.IsBlindShipment = dto.IsBlindShipment;
        shipment.OwnerId = dto.OwnerId;
        shipment.UpdatedDate = DateTime.UtcNow;

        // Lines and Packages are owned by the shipment and replaced wholesale on update.
        shipment.Lines.Clear();
        foreach (var l in dto.Lines)
        {
            shipment.Lines.Add(new ShipmentLine
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                SalesOrderLineId = l.SalesOrderLineId,
                DisplayOrder = l.DisplayOrder
            });
        }
        shipment.Packages.Clear();
        foreach (var p in dto.Packages)
        {
            shipment.Packages.Add(new ShipmentPackage
            {
                PackageNumber = p.PackageNumber,
                WeightKg = p.WeightKg,
                LengthCm = p.LengthCm,
                WidthCm = p.WidthCm,
                HeightCm = p.HeightCm,
                TrackingNumber = p.TrackingNumber,
                Items = p.Items.Select(i => new ShipmentPackageItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            });
        }

        _repo.Update(shipment);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, shipment.Id) ?? shipment;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var shipment = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Shipment {id} not found");

        if (shipment.Status != "Open")
            throw new InvalidOperationException("Only open shipments can be deleted.");

        _repo.Remove(shipment);
        await _repo.SaveChangesAsync();
    }

    public async Task<ShipmentDto> PickAsync(int orgId, int id)
    {
        var shipment = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Shipment {id} not found");

        if (shipment.Status != "Open")
            throw new InvalidOperationException("Only open shipments can be picked.");

        // Picked is a pure status flip — a real TMS/WMS integration point (warehouse staff
        // have physically gathered the items) but stock isn't deducted until Ship, same
        // "deduct at the real fulfillment step" reasoning as the rest of this module.
        shipment.Status = "Picked";
        shipment.UpdatedDate = DateTime.UtcNow;
        _repo.Update(shipment);
        await _repo.SaveChangesAsync();

        return ToDto(shipment);
    }

    public async Task<ShipmentDto> ShipAsync(int orgId, int id)
    {
        var shipment = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Shipment {id} not found");

        if (shipment.Status != "Picked")
            throw new InvalidOperationException("Only picked shipments can be shipped.");

        // Every line must have enough on-hand at the origin warehouse before anything is
        // committed — same "reject up front, don't partially commit" reasoning as
        // StockTransferService/ProductionOrderService.
        foreach (var line in shipment.Lines)
        {
            var onHand = await _movementRepo.GetOnHandAtWarehouseAsync(orgId, line.ProductId, shipment.WarehouseId);
            if (onHand < line.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock of product {line.ProductId} at warehouse: on hand {onHand}, required {line.Quantity}");
        }

        shipment.Status = "Shipped";
        shipment.UpdatedDate = DateTime.UtcNow;
        _repo.Update(shipment);
        await _repo.SaveChangesAsync();

        foreach (var line in shipment.Lines)
        {
            _movementRepo.Add(new StockMovement
            {
                OrganizationId = orgId,
                ProductId = line.ProductId,
                WarehouseId = shipment.WarehouseId,
                MovementType = "Issue",
                Quantity = -line.Quantity,
                EntityType = "Shipment",
                EntityId = shipment.Id,
                MovementDate = DateTime.UtcNow
            });

            // A Transfer Order ships to another company Warehouse — goods arrive there. A
            // Sales Order ships to the customer — goods leave the company, no Receipt.
            if (shipment.SourceType == "TransferOrder")
            {
                _movementRepo.Add(new StockMovement
                {
                    OrganizationId = orgId,
                    ProductId = line.ProductId,
                    WarehouseId = shipment.DestinationWarehouseId!.Value,
                    MovementType = "Receipt",
                    Quantity = line.Quantity,
                    EntityType = "Shipment",
                    EntityId = shipment.Id,
                    MovementDate = DateTime.UtcNow
                });
            }
        }
        await _movementRepo.SaveChangesAsync();

        return ToDto(shipment);
    }

    public async Task<ShipmentDto> DeliverAsync(int orgId, int id)
    {
        var shipment = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Shipment {id} not found");

        if (shipment.Status != "Shipped")
            throw new InvalidOperationException("Only shipped shipments can be marked delivered.");

        shipment.Status = "Delivered";
        shipment.UpdatedDate = DateTime.UtcNow;
        _repo.Update(shipment);
        await _repo.SaveChangesAsync();

        return ToDto(shipment);
    }

    public async Task<ShipmentDto> CancelAsync(int orgId, int id)
    {
        var shipment = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Shipment {id} not found");

        if (shipment.Status != "Open" && shipment.Status != "Picked")
            throw new InvalidOperationException("Only open or picked shipments can be cancelled.");

        shipment.Status = "Cancelled";
        shipment.UpdatedDate = DateTime.UtcNow;
        _repo.Update(shipment);
        await _repo.SaveChangesAsync();

        return ToDto(shipment);
    }

    private static ShipmentDto ToDto(Shipment s) => new()
    {
        Id = s.Id,
        ShipmentNumber = s.ShipmentNumber,
        WarehouseId = s.WarehouseId,
        WarehouseName = s.Warehouse?.Name ?? string.Empty,
        SourceType = s.SourceType,
        SalesOrderId = s.SalesOrderId,
        SalesOrderNumber = s.SalesOrder?.OrderNumber,
        DestinationWarehouseId = s.DestinationWarehouseId,
        DestinationWarehouseName = s.DestinationWarehouse?.Name,
        ShipToName = s.ShipToName,
        ShipToContactName = s.ShipToContactName,
        ShipToEmail = s.ShipToEmail,
        ShipToAddressLine1 = s.ShipToAddressLine1,
        ShipToAddressLine2 = s.ShipToAddressLine2,
        ShipToCity = s.ShipToCity,
        ShipToState = s.ShipToState,
        ShipToPostalCode = s.ShipToPostalCode,
        ShipToCountry = s.ShipToCountry,
        ShipToPhone = s.ShipToPhone,
        ShipToTaxType = s.ShipToTaxType,
        ShipToTaxCountry = s.ShipToTaxCountry,
        ShipToTaxId = s.ShipToTaxId,
        ShipFromName = s.ShipFromName,
        ShipFromContactName = s.ShipFromContactName,
        ShipFromEmail = s.ShipFromEmail,
        ShipFromAddressLine1 = s.ShipFromAddressLine1,
        ShipFromAddressLine2 = s.ShipFromAddressLine2,
        ShipFromCity = s.ShipFromCity,
        ShipFromState = s.ShipFromState,
        ShipFromPostalCode = s.ShipFromPostalCode,
        ShipFromCountry = s.ShipFromCountry,
        ShipFromPhone = s.ShipFromPhone,
        ShipDate = s.ShipDate,
        Carrier = s.Carrier,
        TrackingNumber = s.TrackingNumber,
        IsBlindShipment = s.IsBlindShipment,
        Status = s.Status,
        OwnerId = s.OwnerId,
        OwnerName = s.Owner?.Name ?? string.Empty,
        Lines = s.Lines.OrderBy(l => l.DisplayOrder).Select(l => new ShipmentLineDto
        {
            Id = l.Id,
            ProductId = l.ProductId,
            ProductName = l.Product?.Name ?? string.Empty,
            ProductSku = l.Product?.Sku ?? string.Empty,
            Quantity = l.Quantity,
            SalesOrderLineId = l.SalesOrderLineId,
            DisplayOrder = l.DisplayOrder
        }).ToList(),
        Packages = s.Packages.OrderBy(p => p.PackageNumber).Select(p => new ShipmentPackageDto
        {
            Id = p.Id,
            PackageNumber = p.PackageNumber,
            WeightKg = p.WeightKg,
            LengthCm = p.LengthCm,
            WidthCm = p.WidthCm,
            HeightCm = p.HeightCm,
            TrackingNumber = p.TrackingNumber,
            Items = p.Items.Select(i => new ShipmentPackageItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? string.Empty,
                ProductSku = i.Product?.Sku ?? string.Empty,
                Quantity = i.Quantity
            }).ToList()
        }).ToList()
    };
}
