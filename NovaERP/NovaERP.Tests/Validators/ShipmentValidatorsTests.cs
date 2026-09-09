using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateShipmentDtoValidatorTests
{
    private readonly CreateShipmentDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Required_Fields_Zero_And_No_Lines()
    {
        var dto = new CreateShipmentDto { WarehouseId = 0, SourceType = "Bogus", OwnerId = 0, Lines = new() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.WarehouseId);
        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
        result.ShouldHaveValidationErrorFor(x => x.SourceType);
        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Fails_When_SalesOrder_Type_Missing_SalesOrderId_And_ShipToAddress()
    {
        var dto = new CreateShipmentDto
        {
            WarehouseId = 1,
            SourceType = "SalesOrder",
            OwnerId = 1,
            Lines = new List<CreateShipmentLineDto> { new() { ProductId = 1, Quantity = 1m, DisplayOrder = 1 } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.SalesOrderId);
        result.ShouldHaveValidationErrorFor(x => x.ShipToAddressLine1);
        result.ShouldHaveValidationErrorFor(x => x.ShipToCity);
    }

    [Fact]
    public void Fails_When_TransferOrder_Type_Missing_DestinationWarehouseId()
    {
        var dto = new CreateShipmentDto
        {
            WarehouseId = 1,
            SourceType = "TransferOrder",
            OwnerId = 1,
            Lines = new List<CreateShipmentLineDto> { new() { ProductId = 1, Quantity = 1m, DisplayOrder = 1 } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.DestinationWarehouseId);
    }

    [Fact]
    public void Passes_For_Valid_SalesOrder_Shipment()
    {
        var dto = new CreateShipmentDto
        {
            WarehouseId = 1,
            SourceType = "SalesOrder",
            SalesOrderId = 1,
            ShipToAddressLine1 = "1 Main St",
            ShipToCity = "Mumbai",
            OwnerId = 1,
            Lines = new List<CreateShipmentLineDto> { new() { ProductId = 1, Quantity = 1m, DisplayOrder = 1 } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Passes_For_Valid_TransferOrder_Shipment()
    {
        var dto = new CreateShipmentDto
        {
            WarehouseId = 1,
            SourceType = "TransferOrder",
            DestinationWarehouseId = 2,
            OwnerId = 1,
            Lines = new List<CreateShipmentLineDto> { new() { ProductId = 1, Quantity = 1m, DisplayOrder = 1 } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
