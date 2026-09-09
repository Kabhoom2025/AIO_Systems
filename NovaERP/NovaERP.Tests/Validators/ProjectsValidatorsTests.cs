using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateProjectDtoValidatorTests
{
    private readonly CreateProjectDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Required_Fields_Empty_And_EndDate_Before_StartDate()
    {
        var dto = new CreateProjectDto
        {
            Code = "", Name = "", ManagerId = 0,
            StartDate = DateTime.UtcNow.Date, EndDate = DateTime.UtcNow.Date.AddDays(-1), Budget = -5m
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Code);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.ManagerId);
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
        result.ShouldHaveValidationErrorFor(x => x.Budget);
    }

    [Fact]
    public void Passes_For_Valid_Project()
    {
        var dto = new CreateProjectDto
        {
            Code = "PROJ-X", Name = "New Project", ManagerId = 1,
            StartDate = DateTime.UtcNow.Date, EndDate = DateTime.UtcNow.Date.AddMonths(3), Budget = 10000m
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreateProjectTaskDtoValidatorTests
{
    private readonly CreateProjectTaskDtoValidator _validator = new();

    [Fact]
    public void Fails_When_ProjectId_Zero_Title_Empty_And_Priority_Invalid()
    {
        var dto = new CreateProjectTaskDto { ProjectId = 0, Title = "", Priority = "Bogus", Status = "ToDo" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ProjectId);
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void Passes_For_Valid_Task()
    {
        var dto = new CreateProjectTaskDto { ProjectId = 1, Title = "Do the thing", Priority = "Medium", Status = "ToDo" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
