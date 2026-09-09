using FluentValidation;
using LinkShield.Application.DTOs.Admin;

namespace LinkShield.Application.Validators.Admin;

public class CreateBrandProfileRequestValidator : AbstractValidator<CreateBrandProfileRequest>
{
    public CreateBrandProfileRequestValidator()
    {
        RuleFor(x => x.BrandName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.OfficialDomain).NotEmpty().MaximumLength(255);
    }
}

public class CreateRiskRuleRequestValidator : AbstractValidator<CreateRiskRuleRequest>
{
    public CreateRiskRuleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Weight).InclusiveBetween(0m, 1m);
        RuleFor(x => x.ConditionExpression).NotEmpty();
        RuleFor(x => x.ScoreContribution).InclusiveBetween(0, 100);
    }
}

public class UpdateRiskRuleRequestValidator : AbstractValidator<UpdateRiskRuleRequest>
{
    public UpdateRiskRuleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Weight).InclusiveBetween(0m, 1m);
        RuleFor(x => x.ConditionExpression).NotEmpty();
        RuleFor(x => x.ScoreContribution).InclusiveBetween(0, 100);
    }
}
