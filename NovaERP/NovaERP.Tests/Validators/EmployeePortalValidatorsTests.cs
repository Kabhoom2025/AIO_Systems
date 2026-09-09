using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateMyLeaveRequestDtoValidatorTests
{
    private readonly CreateMyLeaveRequestDtoValidator _validator = new();

    [Fact]
    public void Fails_When_LeaveTypeId_Zero_Days_Zero_And_EndDate_Before_StartDate()
    {
        var dto = new CreateMyLeaveRequestDto
        {
            LeaveTypeId = 0, DaysRequested = 0,
            StartDate = DateTime.UtcNow.Date, EndDate = DateTime.UtcNow.Date.AddDays(-1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.LeaveTypeId);
        result.ShouldHaveValidationErrorFor(x => x.DaysRequested);
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Passes_For_Valid_Request()
    {
        var dto = new CreateMyLeaveRequestDto
        {
            LeaveTypeId = 1, DaysRequested = 2,
            StartDate = DateTime.UtcNow.Date, EndDate = DateTime.UtcNow.Date.AddDays(1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
