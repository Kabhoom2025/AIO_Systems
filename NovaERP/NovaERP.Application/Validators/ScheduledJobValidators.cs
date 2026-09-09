using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class UpdateScheduledJobDtoValidator : AbstractValidator<UpdateScheduledJobDto>
{
    public UpdateScheduledJobDtoValidator()
    {
        RuleFor(x => x.CronExpression).NotEmpty()
            .Must(cron => cron.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 5)
            .WithMessage("CronExpression must be a standard 5-field cron expression.");
    }
}
