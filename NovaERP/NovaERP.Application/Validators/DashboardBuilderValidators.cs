using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public static class WidgetTypes
{
    public static readonly string[] All =
    {
        "SalesOrderStatusSummary", "ServiceTicketStatusSummary", "PosSalesTotal", "ProjectTaskStatusSummary",
        "PurchaseOrderStatusSummary", "ShipmentStatusSummary", "RfqStatusSummary"
    };
}

public static class WidgetSizes
{
    public static readonly string[] All = { "Small", "Medium", "Large" };
}

public class CreateDashboardDtoValidator : AbstractValidator<CreateDashboardDto>
{
    public CreateDashboardDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateDashboardDtoValidator : AbstractValidator<UpdateDashboardDto>
{
    public UpdateDashboardDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class AddWidgetDtoValidator : AbstractValidator<AddWidgetDto>
{
    public AddWidgetDtoValidator()
    {
        RuleFor(x => x.WidgetType).Must(t => WidgetTypes.All.Contains(t))
            .WithMessage($"WidgetType must be one of: {string.Join(", ", WidgetTypes.All)}");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SizeOption).Must(s => WidgetSizes.All.Contains(s))
            .WithMessage($"SizeOption must be one of: {string.Join(", ", WidgetSizes.All)}");
    }
}

public class ReorderWidgetsDtoValidator : AbstractValidator<ReorderWidgetsDto>
{
    public ReorderWidgetsDtoValidator()
    {
        RuleForEach(x => x.Widgets).ChildRules(w =>
        {
            w.RuleFor(e => e.WidgetId).GreaterThan(0);
            w.RuleFor(e => e.SizeOption).Must(s => WidgetSizes.All.Contains(s))
                .WithMessage($"SizeOption must be one of: {string.Join(", ", WidgetSizes.All)}");
        });
    }
}
