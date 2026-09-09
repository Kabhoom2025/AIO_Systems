using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateLeadDtoValidatorTests
{
    private readonly CreateLeadDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Name_Empty_Or_Owner_Missing()
    {
        var dto = new CreateLeadDto { Name = "", OwnerId = 0 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
    }

    [Fact]
    public void Fails_When_Email_Invalid()
    {
        var dto = new CreateLeadDto { Name = "Suresh Kumar", OwnerId = 1, Email = "not-an-email" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Passes_For_Valid_Lead()
    {
        var dto = new CreateLeadDto { Name = "Suresh Kumar", OwnerId = 1, Email = "suresh@example.com" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class UpdateLeadDtoValidatorTests
{
    private readonly UpdateLeadDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Status_Is_Converted()
    {
        var dto = new UpdateLeadDto { Name = "Suresh Kumar", OwnerId = 1, Status = "Converted" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Fails_When_Status_Not_In_Allowed_List()
    {
        var dto = new UpdateLeadDto { Name = "Suresh Kumar", OwnerId = 1, Status = "Bogus" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Passes_For_Valid_Status_Transition()
    {
        var dto = new UpdateLeadDto { Name = "Suresh Kumar", OwnerId = 1, Status = "Qualified" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class ConvertLeadDtoValidatorTests
{
    private readonly ConvertLeadDtoValidator _validator = new();

    [Fact]
    public void Fails_When_CreateOpportunity_True_But_Name_And_Amount_Missing()
    {
        var dto = new ConvertLeadDto { CreateOpportunity = true };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.OpportunityName);
        result.ShouldHaveValidationErrorFor(x => x.OpportunityAmount);
    }

    [Fact]
    public void Passes_When_CreateOpportunity_False()
    {
        var dto = new ConvertLeadDto { CreateOpportunity = false };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Passes_When_CreateOpportunity_True_With_Name_And_Amount()
    {
        var dto = new ConvertLeadDto { CreateOpportunity = true, OpportunityName = "New Deal", OpportunityAmount = 1000m };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreateOpportunityDtoValidatorTests
{
    private readonly CreateOpportunityDtoValidator _validator = new();

    [Fact]
    public void Fails_When_AccountId_Zero_Or_Stage_Invalid()
    {
        var dto = new CreateOpportunityDto { AccountId = 0, Name = "Deal", Amount = 100m, Stage = "Bogus", OwnerId = 1 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AccountId);
        result.ShouldHaveValidationErrorFor(x => x.Stage);
    }

    [Fact]
    public void Passes_For_Valid_Opportunity()
    {
        var dto = new CreateOpportunityDto { AccountId = 1, Name = "Deal", Amount = 100m, Stage = "Qualification", OwnerId = 1 };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
