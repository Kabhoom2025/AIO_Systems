using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreateWorkflowStepDefinitionDtoValidator : AbstractValidator<CreateWorkflowStepDefinitionDto>
{
    public CreateWorkflowStepDefinitionDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MinAmount).GreaterThanOrEqualTo(0m).When(x => x.MinAmount.HasValue);
    }
}

public class CreateWorkflowDefinitionDtoValidator : AbstractValidator<CreateWorkflowDefinitionDto>
{
    public CreateWorkflowDefinitionDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Steps).NotEmpty().WithMessage("A workflow definition must have at least one step.");
        RuleForEach(x => x.Steps).SetValidator(new CreateWorkflowStepDefinitionDtoValidator());
        RuleFor(x => x.Steps)
            .Must(steps => steps.Select(s => s.StepOrder).Distinct().Count() == steps.Count)
            .WithMessage("StepOrder must be unique within a workflow definition.")
            .When(x => x.Steps.Count > 0);
    }
}

public class UpdateWorkflowDefinitionDtoValidator : AbstractValidator<UpdateWorkflowDefinitionDto>
{
    public UpdateWorkflowDefinitionDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Steps).NotEmpty().WithMessage("A workflow definition must have at least one step.");
        RuleForEach(x => x.Steps).SetValidator(new CreateWorkflowStepDefinitionDtoValidator());
        RuleFor(x => x.Steps)
            .Must(steps => steps.Select(s => s.StepOrder).Distinct().Count() == steps.Count)
            .WithMessage("StepOrder must be unique within a workflow definition.")
            .When(x => x.Steps.Count > 0);
    }
}

public class StartWorkflowDtoValidator : AbstractValidator<StartWorkflowDto>
{
    public StartWorkflowDtoValidator()
    {
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0m).When(x => x.Amount.HasValue);
    }
}

public class WorkflowActionDtoValidator : AbstractValidator<WorkflowActionDto>
{
    public WorkflowActionDtoValidator()
    {
        RuleFor(x => x.Comments).MaximumLength(1000);
    }
}
