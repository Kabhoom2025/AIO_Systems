using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public static class ProjectStatuses
{
    public static readonly string[] All = { "Planning", "Active", "OnHold", "Completed", "Cancelled" };
}

public static class ProjectTaskStatuses
{
    public static readonly string[] All = { "ToDo", "InProgress", "Done", "Blocked" };
}

public static class ProjectTaskPriorities
{
    public static readonly string[] All = { "Low", "Medium", "High" };
}

public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ManagerId).GreaterThan(0);
        RuleFor(x => x.Budget).GreaterThanOrEqualTo(0).When(x => x.Budget.HasValue);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate.HasValue)
            .WithMessage("End date must be on or after the start date.");
    }
}

public class UpdateProjectDtoValidator : AbstractValidator<UpdateProjectDto>
{
    public UpdateProjectDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ManagerId).GreaterThan(0);
        RuleFor(x => x.Budget).GreaterThanOrEqualTo(0).When(x => x.Budget.HasValue);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate.HasValue)
            .WithMessage("End date must be on or after the start date.");
        RuleFor(x => x.Status).Must(s => ProjectStatuses.All.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", ProjectStatuses.All)}");
    }
}

public class CreateProjectTaskDtoValidator : AbstractValidator<CreateProjectTaskDto>
{
    public CreateProjectTaskDtoValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Priority).Must(p => ProjectTaskPriorities.All.Contains(p))
            .WithMessage($"Priority must be one of: {string.Join(", ", ProjectTaskPriorities.All)}");
        RuleFor(x => x.Status).Must(s => ProjectTaskStatuses.All.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", ProjectTaskStatuses.All)}");
    }
}

public class UpdateProjectTaskDtoValidator : AbstractValidator<UpdateProjectTaskDto>
{
    public UpdateProjectTaskDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Priority).Must(p => ProjectTaskPriorities.All.Contains(p))
            .WithMessage($"Priority must be one of: {string.Join(", ", ProjectTaskPriorities.All)}");
        RuleFor(x => x.Status).Must(s => ProjectTaskStatuses.All.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", ProjectTaskStatuses.All)}");
    }
}
