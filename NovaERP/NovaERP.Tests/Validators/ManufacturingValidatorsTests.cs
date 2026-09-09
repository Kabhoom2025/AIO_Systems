using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateBillOfMaterialDtoValidatorTests
{
    private readonly CreateBillOfMaterialDtoValidator _validator = new();

    [Fact]
    public void Fails_When_ProductId_Zero_And_No_Components()
    {
        var dto = new CreateBillOfMaterialDto { ProductId = 0, Components = new() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ProductId);
        result.ShouldHaveValidationErrorFor(x => x.Components);
    }

    [Fact]
    public void Passes_For_Valid_Bom()
    {
        var dto = new CreateBillOfMaterialDto
        {
            ProductId = 1,
            Components = new List<CreateBomComponentDto>
            {
                new() { ComponentProductId = 2, Quantity = 4m, DisplayOrder = 1 }
            }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreateProductionOrderDtoValidatorTests
{
    private readonly CreateProductionOrderDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Ids_Zero_And_Quantity_Not_Positive()
    {
        var dto = new CreateProductionOrderDto { ProductId = 0, WarehouseId = 0, Quantity = 0, OwnerId = 0 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ProductId);
        result.ShouldHaveValidationErrorFor(x => x.WarehouseId);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
    }

    [Fact]
    public void Passes_For_Valid_ProductionOrder()
    {
        var dto = new CreateProductionOrderDto { ProductId = 1, WarehouseId = 1, Quantity = 10m, OwnerId = 1 };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
