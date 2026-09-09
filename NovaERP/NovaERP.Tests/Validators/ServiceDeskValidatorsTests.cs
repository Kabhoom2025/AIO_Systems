using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateTicketCategoryDtoValidatorTests
{
    private readonly CreateTicketCategoryDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Name_And_Code_Empty()
    {
        var dto = new CreateTicketCategoryDto { Name = "", Code = "" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Passes_For_Valid_Category()
    {
        var dto = new CreateTicketCategoryDto { Name = "Network", Code = "NETWORK" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreateServiceTicketDtoValidatorTests
{
    private readonly CreateServiceTicketDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Subject_Empty_CategoryId_Zero_RequesterId_Zero_And_Priority_Invalid()
    {
        var dto = new CreateServiceTicketDto { Subject = "", Description = "", CategoryId = 0, RequesterId = 0, Priority = "Urgent" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Subject);
        result.ShouldHaveValidationErrorFor(x => x.Description);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        result.ShouldHaveValidationErrorFor(x => x.RequesterId);
        result.ShouldHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void Passes_For_Valid_Ticket()
    {
        var dto = new CreateServiceTicketDto { Subject = "Printer not working", Description = "Paper jam error", CategoryId = 1, RequesterId = 1, Priority = "High" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
