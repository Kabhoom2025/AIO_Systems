using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateEmployeeDtoValidatorTests
{
    private readonly CreateEmployeeDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Required_Fields_Empty_And_EmploymentType_Invalid()
    {
        var dto = new CreateEmployeeDto
        {
            DepartmentId = 0, FirstName = "", LastName = "", Email = "not-an-email",
            JobTitle = "", EmploymentType = "Bogus"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.JobTitle);
        result.ShouldHaveValidationErrorFor(x => x.EmploymentType);
    }

    [Fact]
    public void Passes_For_Valid_Employee()
    {
        var dto = new CreateEmployeeDto
        {
            DepartmentId = 1, FirstName = "Jane", LastName = "Doe", Email = "jane.doe@example.com",
            JobTitle = "Analyst", EmploymentType = "Full-Time"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreateLeaveRequestDtoValidatorTests
{
    private readonly CreateLeaveRequestDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Ids_Zero_Days_Zero_And_EndDate_Before_StartDate()
    {
        var dto = new CreateLeaveRequestDto
        {
            EmployeeId = 0, LeaveTypeId = 0, DaysRequested = 0,
            StartDate = DateTime.UtcNow.Date, EndDate = DateTime.UtcNow.Date.AddDays(-1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
        result.ShouldHaveValidationErrorFor(x => x.LeaveTypeId);
        result.ShouldHaveValidationErrorFor(x => x.DaysRequested);
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Passes_For_Valid_Request()
    {
        var dto = new CreateLeaveRequestDto
        {
            EmployeeId = 1, LeaveTypeId = 1, DaysRequested = 2,
            StartDate = DateTime.UtcNow.Date, EndDate = DateTime.UtcNow.Date.AddDays(1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
