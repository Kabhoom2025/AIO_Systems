using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateTaxCodeDtoValidatorTests
{
    private readonly CreateTaxCodeDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Code_And_Name_Are_Empty()
    {
        var dto = new CreateTaxCodeDto { Code = "", Name = "", Components = { new CreateTaxComponentDto { Name = "CGST", RatePercent = 9m } } };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Code);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Fails_When_No_Components()
    {
        var dto = new CreateTaxCodeDto { Code = "GST18", Name = "GST 18%", Components = new List<CreateTaxComponentDto>() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Components);
    }

    [Fact]
    public void Fails_When_Component_Rate_Is_Negative()
    {
        var dto = new CreateTaxCodeDto
        {
            Code = "GST18",
            Name = "GST 18%",
            Components = { new CreateTaxComponentDto { Name = "CGST", RatePercent = -1m } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("Components[0].RatePercent");
    }

    [Fact]
    public void Passes_For_Valid_Compound_Tax_Code()
    {
        var dto = new CreateTaxCodeDto
        {
            Code = "GST18",
            Name = "GST 18%",
            IsActive = true,
            Components =
            {
                new CreateTaxComponentDto { Name = "CGST", RatePercent = 9m, DisplayOrder = 1 },
                new CreateTaxComponentDto { Name = "SGST", RatePercent = 9m, DisplayOrder = 2 }
            }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class UpdateTaxCodeDtoValidatorTests
{
    private readonly UpdateTaxCodeDtoValidator _validator = new();

    [Fact]
    public void Fails_When_No_Components()
    {
        var dto = new UpdateTaxCodeDto { Name = "GST 18%", Components = new List<CreateTaxComponentDto>() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Components);
    }

    [Fact]
    public void Passes_For_Valid_Update()
    {
        var dto = new UpdateTaxCodeDto
        {
            Name = "GST 18%",
            IsActive = true,
            Components = { new CreateTaxComponentDto { Name = "CGST", RatePercent = 9m, DisplayOrder = 1 } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
