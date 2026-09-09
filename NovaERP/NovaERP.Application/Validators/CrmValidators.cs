using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public static class CrmStatusValues
{
    public static readonly string[] LeadStatuses = { "New", "Contacted", "Qualified", "Converted", "Lost" };
    public static readonly string[] OpportunityStages = { "Qualification", "Proposal", "Negotiation", "Won", "Lost" };
}

public class CreateLeadDtoValidator : AbstractValidator<CreateLeadDto>
{
    public CreateLeadDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.OwnerId).GreaterThan(0);
    }
}

public class UpdateLeadDtoValidator : AbstractValidator<UpdateLeadDto>
{
    public UpdateLeadDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Status)
            .Must(s => CrmStatusValues.LeadStatuses.Contains(s) && s != "Converted")
            .WithMessage("Status must be one of New, Contacted, Qualified, Lost — use the convert endpoint to mark a lead Converted.");
    }
}

public class ConvertLeadDtoValidator : AbstractValidator<ConvertLeadDto>
{
    public ConvertLeadDtoValidator()
    {
        RuleFor(x => x.OpportunityName)
            .NotEmpty()
            .When(x => x.CreateOpportunity)
            .WithMessage("Opportunity name is required when creating an opportunity during conversion.");
        RuleFor(x => x.OpportunityAmount)
            .NotNull().GreaterThanOrEqualTo(0)
            .When(x => x.CreateOpportunity)
            .WithMessage("Opportunity amount is required when creating an opportunity during conversion.");
    }
}

public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
{
    public CreateAccountDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OwnerId).GreaterThan(0);
    }
}

public class UpdateAccountDtoValidator : AbstractValidator<UpdateAccountDto>
{
    public UpdateAccountDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OwnerId).GreaterThan(0);
    }
}

public class CreateContactDtoValidator : AbstractValidator<CreateContactDto>
{
    public CreateContactDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.OwnerId).GreaterThan(0);
    }
}

public class UpdateContactDtoValidator : AbstractValidator<UpdateContactDto>
{
    public UpdateContactDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.OwnerId).GreaterThan(0);
    }
}

public class CreateOpportunityDtoValidator : AbstractValidator<CreateOpportunityDto>
{
    public CreateOpportunityDtoValidator()
    {
        RuleFor(x => x.AccountId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Stage).Must(s => CrmStatusValues.OpportunityStages.Contains(s));
        RuleFor(x => x.OwnerId).GreaterThan(0);
    }
}

public class UpdateOpportunityDtoValidator : AbstractValidator<UpdateOpportunityDto>
{
    public UpdateOpportunityDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Stage).Must(s => CrmStatusValues.OpportunityStages.Contains(s));
        RuleFor(x => x.OwnerId).GreaterThan(0);
    }
}
