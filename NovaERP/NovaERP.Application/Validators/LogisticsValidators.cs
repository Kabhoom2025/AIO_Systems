using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreateVehicleDtoValidator : AbstractValidator<CreateVehicleDto>
{
    public CreateVehicleDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CapacityKg).GreaterThan(0);
    }
}

public class UpdateVehicleDtoValidator : AbstractValidator<UpdateVehicleDto>
{
    public UpdateVehicleDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CapacityKg).GreaterThan(0);
    }
}

public class CreateDeliveryLoadDtoValidator : AbstractValidator<CreateDeliveryLoadDto>
{
    public CreateDeliveryLoadDtoValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
    }
}

public class AssignShipmentDtoValidator : AbstractValidator<AssignShipmentDto>
{
    public AssignShipmentDtoValidator()
    {
        RuleFor(x => x.ShipmentId).GreaterThan(0);
    }
}
