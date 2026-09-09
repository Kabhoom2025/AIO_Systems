using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateStoreDtoValidatorTests
{
    private readonly CreateStoreDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Name_Code_Branch_Warehouse_Empty()
    {
        var dto = new CreateStoreDto { Name = "", Code = "", BranchId = 0, WarehouseId = 0 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Code);
        result.ShouldHaveValidationErrorFor(x => x.BranchId);
        result.ShouldHaveValidationErrorFor(x => x.WarehouseId);
    }

    [Fact]
    public void Passes_For_Valid_Store()
    {
        var dto = new CreateStoreDto { Name = "Downtown Store", Code = "STORE-DT", BranchId = 1, WarehouseId = 1 };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
