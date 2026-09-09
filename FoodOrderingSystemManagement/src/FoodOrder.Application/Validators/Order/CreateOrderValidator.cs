using FluentValidation;
using FoodOrder.Application.DTOs.Order;

namespace FoodOrder.Application.Validators.Order;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.FoodItemId)
                .GreaterThan(0).WithMessage("Each order item must reference a valid food item.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.");
        });
    }
}
