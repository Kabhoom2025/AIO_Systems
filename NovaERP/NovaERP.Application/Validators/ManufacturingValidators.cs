using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreateBomComponentDtoValidator : AbstractValidator<CreateBomComponentDto>
{
    public CreateBomComponentDtoValidator()
    {
        RuleFor(x => x.ComponentProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class CreateBillOfMaterialDtoValidator : AbstractValidator<CreateBillOfMaterialDto>
{
    public CreateBillOfMaterialDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Components).NotEmpty().WithMessage("A bill of materials needs at least one component.");
        RuleForEach(x => x.Components).SetValidator(new CreateBomComponentDtoValidator());
    }
}

public class UpdateBillOfMaterialDtoValidator : AbstractValidator<UpdateBillOfMaterialDto>
{
    public UpdateBillOfMaterialDtoValidator()
    {
        RuleFor(x => x.Components).NotEmpty().WithMessage("A bill of materials needs at least one component.");
        RuleForEach(x => x.Components).SetValidator(new CreateBomComponentDtoValidator());
    }
}

public class CreateProductionOrderDtoValidator : AbstractValidator<CreateProductionOrderDto>
{
    public CreateProductionOrderDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
    }
}

public class UpdateProductionOrderDtoValidator : AbstractValidator<UpdateProductionOrderDto>
{
    public UpdateProductionOrderDtoValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
    }
}
