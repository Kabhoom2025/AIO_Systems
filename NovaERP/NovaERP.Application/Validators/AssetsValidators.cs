using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public static class AssetStatuses
{
    public static readonly string[] All = { "Available", "Assigned", "UnderMaintenance", "Retired" };
}

public class CreateAssetCategoryDtoValidator : AbstractValidator<CreateAssetCategoryDto>
{
    public CreateAssetCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}

public class UpdateAssetCategoryDtoValidator : AbstractValidator<UpdateAssetCategoryDto>
{
    public UpdateAssetCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateAssetDtoValidator : AbstractValidator<CreateAssetDto>
{
    public CreateAssetDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.PurchaseCost).GreaterThanOrEqualTo(0).When(x => x.PurchaseCost.HasValue);
    }
}

public class UpdateAssetDtoValidator : AbstractValidator<UpdateAssetDto>
{
    public UpdateAssetDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.PurchaseCost).GreaterThanOrEqualTo(0).When(x => x.PurchaseCost.HasValue);
    }
}

public class AssignAssetDtoValidator : AbstractValidator<AssignAssetDto>
{
    public AssignAssetDtoValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0);
    }
}
