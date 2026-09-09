using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateProductDtoValidatorTests
{
    private readonly CreateProductDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Sku_And_Name_Empty()
    {
        var dto = new CreateProductDto { Sku = "", Name = "", UnitOfMeasure = "" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Sku);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.UnitOfMeasure);
    }

    [Fact]
    public void Passes_For_Valid_Product()
    {
        var dto = new CreateProductDto { Sku = "STEEL-2MM", Name = "Cold-rolled steel sheet", UnitOfMeasure = "EA", UnitCost = 850m };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreateStockMovementDtoValidatorTests
{
    private readonly CreateStockMovementDtoValidator _validator = new();

    [Fact]
    public void Fails_When_ProductId_Zero_Type_Invalid_Or_Quantity_Zero()
    {
        var dto = new CreateStockMovementDto { ProductId = 0, MovementType = "Bogus", Quantity = 0 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ProductId);
        result.ShouldHaveValidationErrorFor(x => x.MovementType);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Passes_For_Valid_Adjustment()
    {
        var dto = new CreateStockMovementDto { ProductId = 1, MovementType = "Adjustment", Quantity = -5m, Notes = "Stock take correction" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
