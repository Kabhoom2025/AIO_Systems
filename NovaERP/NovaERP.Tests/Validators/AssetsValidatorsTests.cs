using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateAssetCategoryDtoValidatorTests
{
    private readonly CreateAssetCategoryDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Name_And_Code_Empty()
    {
        var dto = new CreateAssetCategoryDto { Name = "", Code = "" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Passes_For_Valid_Category()
    {
        var dto = new CreateAssetCategoryDto { Name = "Monitor", Code = "MONITOR" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreateAssetDtoValidatorTests
{
    private readonly CreateAssetDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Name_Empty_CategoryId_Zero_And_Cost_Negative()
    {
        var dto = new CreateAssetDto { Name = "", CategoryId = 0, PurchaseCost = -1 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseCost);
    }

    [Fact]
    public void Passes_For_Valid_Asset()
    {
        var dto = new CreateAssetDto { Name = "Dell Monitor 24\"", CategoryId = 1, PurchaseCost = 15000m };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
