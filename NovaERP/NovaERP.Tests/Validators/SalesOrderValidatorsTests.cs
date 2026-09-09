using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateSalesOrderDtoValidatorTests
{
    private readonly CreateSalesOrderDtoValidator _validator = new();

    [Fact]
    public void Fails_When_AccountId_Owner_Or_Lines_Missing()
    {
        var dto = new CreateSalesOrderDto { AccountId = 0, OwnerId = 0, Lines = new List<CreateSalesOrderLineDto>() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AccountId);
        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Fails_When_Line_Quantity_Zero()
    {
        var dto = new CreateSalesOrderDto
        {
            AccountId = 1, OwnerId = 1,
            Lines = { new CreateSalesOrderLineDto { ItemName = "Widget", Quantity = 0, UnitPrice = 10m } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("Lines[0].Quantity");
    }

    [Fact]
    public void Passes_For_Valid_Order()
    {
        var dto = new CreateSalesOrderDto
        {
            AccountId = 1, OwnerId = 1,
            Lines = { new CreateSalesOrderLineDto { ItemName = "Widget", Quantity = 5m, UnitPrice = 10m, DisplayOrder = 1 } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class UpdateSalesOrderDtoValidatorTests
{
    private readonly UpdateSalesOrderDtoValidator _validator = new();

    [Fact]
    public void Fails_When_No_Lines()
    {
        var dto = new UpdateSalesOrderDto { OwnerId = 1, Lines = new List<CreateSalesOrderLineDto>() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Passes_For_Valid_Update()
    {
        var dto = new UpdateSalesOrderDto
        {
            OwnerId = 1,
            Lines = { new CreateSalesOrderLineDto { ItemName = "Widget", Quantity = 5m, UnitPrice = 10m, DisplayOrder = 1 } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
