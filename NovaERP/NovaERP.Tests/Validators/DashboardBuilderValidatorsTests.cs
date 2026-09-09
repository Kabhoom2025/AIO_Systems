using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateDashboardDtoValidatorTests
{
    private readonly CreateDashboardDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Name_Empty()
    {
        var result = _validator.TestValidate(new CreateDashboardDto { Name = "" });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Passes_For_Valid_Dashboard()
    {
        var result = _validator.TestValidate(new CreateDashboardDto { Name = "My Dashboard" });
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class AddWidgetDtoValidatorTests
{
    private readonly AddWidgetDtoValidator _validator = new();

    [Fact]
    public void Fails_For_Unknown_WidgetType_Or_SizeOption()
    {
        var dto = new AddWidgetDto { WidgetType = "NotARealWidget", Title = "Test", SizeOption = "Huge" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.WidgetType);
        result.ShouldHaveValidationErrorFor(x => x.SizeOption);
    }

    [Fact]
    public void Passes_For_Valid_Widget()
    {
        var dto = new AddWidgetDto { WidgetType = "PosSalesTotal", Title = "POS Sales", SizeOption = "Small" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
