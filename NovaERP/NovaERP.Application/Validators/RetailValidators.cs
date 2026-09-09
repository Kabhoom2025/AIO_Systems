using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreateStoreDtoValidator : AbstractValidator<CreateStoreDto>
{
    public CreateStoreDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
    }
}

public class UpdateStoreDtoValidator : AbstractValidator<UpdateStoreDto>
{
    public UpdateStoreDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
    }
}
