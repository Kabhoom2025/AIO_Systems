using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateWarehouseDtoValidatorTests
{
    private readonly CreateWarehouseDtoValidator _validator = new();

    [Fact]
    public void Fails_When_BranchId_Zero_Name_And_Code_Empty()
    {
        var dto = new CreateWarehouseDto { BranchId = 0, Name = "", Code = "" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BranchId);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Passes_For_Valid_Warehouse()
    {
        var dto = new CreateWarehouseDto { BranchId = 1, Name = "Bengaluru Main Warehouse", Code = "WH-BLR" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreateStockTransferDtoValidatorTests
{
    private readonly CreateStockTransferDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Ids_Zero_And_Quantity_Not_Positive()
    {
        var dto = new CreateStockTransferDto { ProductId = 0, FromWarehouseId = 0, ToWarehouseId = 0, Quantity = 0 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ProductId);
        result.ShouldHaveValidationErrorFor(x => x.FromWarehouseId);
        result.ShouldHaveValidationErrorFor(x => x.ToWarehouseId);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Fails_When_From_And_To_Warehouse_Are_The_Same()
    {
        var dto = new CreateStockTransferDto { ProductId = 1, FromWarehouseId = 1, ToWarehouseId = 1, Quantity = 10m };

        var result = _validator.TestValidate(dto);

        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("must be different"));
    }

    [Fact]
    public void Passes_For_Valid_Transfer()
    {
        var dto = new CreateStockTransferDto { ProductId = 1, FromWarehouseId = 1, ToWarehouseId = 2, Quantity = 10m };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
