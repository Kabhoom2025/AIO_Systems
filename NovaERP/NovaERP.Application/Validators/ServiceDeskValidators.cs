using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public static class TicketPriorities
{
    public static readonly string[] All = { "Low", "Medium", "High", "Critical" };
}

public static class TicketStatuses
{
    public static readonly string[] All = { "Open", "InProgress", "Resolved", "Closed" };
}

public class CreateTicketCategoryDtoValidator : AbstractValidator<CreateTicketCategoryDto>
{
    public CreateTicketCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}

public class UpdateTicketCategoryDtoValidator : AbstractValidator<UpdateTicketCategoryDto>
{
    public UpdateTicketCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateServiceTicketDtoValidator : AbstractValidator<CreateServiceTicketDto>
{
    public CreateServiceTicketDtoValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.RequesterId).GreaterThan(0);
        RuleFor(x => x.Priority).Must(p => TicketPriorities.All.Contains(p))
            .WithMessage($"Priority must be one of: {string.Join(", ", TicketPriorities.All)}");
    }
}

public class UpdateServiceTicketDtoValidator : AbstractValidator<UpdateServiceTicketDto>
{
    public UpdateServiceTicketDtoValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Priority).Must(p => TicketPriorities.All.Contains(p))
            .WithMessage($"Priority must be one of: {string.Join(", ", TicketPriorities.All)}");
    }
}

public class AssignTicketDtoValidator : AbstractValidator<AssignTicketDto>
{
    public AssignTicketDtoValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0);
    }
}

public class ResolveTicketDtoValidator : AbstractValidator<ResolveTicketDto>
{
    public ResolveTicketDtoValidator()
    {
        RuleFor(x => x.ResolutionNotes).NotEmpty();
    }
}
