using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

/// <summary>Mirrors CreateLeaveRequestDtoValidator minus the EmployeeId rule — the portal
/// never accepts an EmployeeId from the client.</summary>
public class CreateMyLeaveRequestDtoValidator : AbstractValidator<CreateMyLeaveRequestDto>
{
    public CreateMyLeaveRequestDtoValidator()
    {
        RuleFor(x => x.LeaveTypeId).GreaterThan(0);
        RuleFor(x => x.DaysRequested).GreaterThan(0);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after the start date.");
    }
}
