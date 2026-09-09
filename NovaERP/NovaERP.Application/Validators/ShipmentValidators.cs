using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public static class ShipmentSourceTypes
{
    public static readonly string[] All = { "SalesOrder", "TransferOrder" };
}

public class CreateShipmentLineDtoValidator : AbstractValidator<CreateShipmentLineDto>
{
    public CreateShipmentLineDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class CreateShipmentPackageDtoValidator : AbstractValidator<CreateShipmentPackageDto>
{
    public CreateShipmentPackageDtoValidator()
    {
        RuleFor(x => x.PackageNumber).GreaterThan(0);
        RuleFor(x => x.WeightKg).GreaterThanOrEqualTo(0).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.LengthCm).GreaterThanOrEqualTo(0).When(x => x.LengthCm.HasValue);
        RuleFor(x => x.WidthCm).GreaterThanOrEqualTo(0).When(x => x.WidthCm.HasValue);
        RuleFor(x => x.HeightCm).GreaterThanOrEqualTo(0).When(x => x.HeightCm.HasValue);
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

public class CreateShipmentDtoValidator : AbstractValidator<CreateShipmentDto>
{
    public CreateShipmentDtoValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.SourceType).Must(t => ShipmentSourceTypes.All.Contains(t))
            .WithMessage($"SourceType must be one of: {string.Join(", ", ShipmentSourceTypes.All)}");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A shipment needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreateShipmentLineDtoValidator());
        RuleForEach(x => x.Packages).SetValidator(new CreateShipmentPackageDtoValidator());

        RuleFor(x => x.SalesOrderId)
            .NotNull().WithMessage("SalesOrderId is required when SourceType is SalesOrder.")
            .When(x => x.SourceType == "SalesOrder");
        RuleFor(x => x.DestinationWarehouseId)
            .NotNull().WithMessage("DestinationWarehouseId is required when SourceType is TransferOrder.")
            .When(x => x.SourceType == "TransferOrder");
        RuleFor(x => x.ShipToAddressLine1)
            .NotEmpty().WithMessage("ShipToAddressLine1 is required when SourceType is SalesOrder.")
            .When(x => x.SourceType == "SalesOrder");
        RuleFor(x => x.ShipToCity)
            .NotEmpty().WithMessage("ShipToCity is required when SourceType is SalesOrder.")
            .When(x => x.SourceType == "SalesOrder");
    }
}

public class UpdateShipmentDtoValidator : AbstractValidator<UpdateShipmentDto>
{
    public UpdateShipmentDtoValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A shipment needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreateShipmentLineDtoValidator());
        RuleForEach(x => x.Packages).SetValidator(new CreateShipmentPackageDtoValidator());
    }
}
