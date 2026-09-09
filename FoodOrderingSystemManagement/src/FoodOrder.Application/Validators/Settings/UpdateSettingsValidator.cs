using FluentValidation;
using FoodOrder.Application.DTOs.Settings;

namespace FoodOrder.Application.Validators.Settings;

public class UpdateSettingsValidator : AbstractValidator<UpdateSettingsDto>
{
    public UpdateSettingsValidator()
    {
        RuleFor(x => x.RestaurantName)
            .NotEmpty().WithMessage("Restaurant name is required.")
            .MaximumLength(200).WithMessage("Restaurant name cannot exceed 200 characters.");

        RuleFor(x => x.TaxPercentage)
            .GreaterThanOrEqualTo(0).WithMessage("Tax percentage cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Tax percentage cannot exceed 100.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.GSTNumber)
            .MaximumLength(50).WithMessage("GST number cannot exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.GSTNumber));
    }
}
