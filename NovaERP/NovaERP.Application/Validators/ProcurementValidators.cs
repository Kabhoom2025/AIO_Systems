using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreateVendorDtoValidator : AbstractValidator<CreateVendorDto>
{
    public CreateVendorDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.ContactEmail));
        RuleFor(x => x.OwnerId).GreaterThan(0);
    }
}

public class UpdateVendorDtoValidator : AbstractValidator<UpdateVendorDto>
{
    public UpdateVendorDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.ContactEmail));
        RuleFor(x => x.OwnerId).GreaterThan(0);
    }
}

public class CreateRfqItemDtoValidator : AbstractValidator<CreateRfqItemDto>
{
    public CreateRfqItemDtoValidator()
    {
        RuleFor(x => x.ItemName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class CreateRfqRequestDtoValidator : AbstractValidator<CreateRfqRequestDto>
{
    public CreateRfqRequestDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("An RFQ needs at least one item.");
        RuleForEach(x => x.Items).SetValidator(new CreateRfqItemDtoValidator());
        RuleFor(x => x.InvitedVendorIds).NotEmpty().WithMessage("Invite at least one vendor to quote.");
    }
}

public class UpdateRfqRequestDtoValidator : AbstractValidator<UpdateRfqRequestDto>
{
    public UpdateRfqRequestDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("An RFQ needs at least one item.");
        RuleForEach(x => x.Items).SetValidator(new CreateRfqItemDtoValidator());
        RuleFor(x => x.InvitedVendorIds).NotEmpty().WithMessage("Invite at least one vendor to quote.");
    }
}

public class RecordRfqQuoteDtoValidator : AbstractValidator<RecordRfqQuoteDto>
{
    public RecordRfqQuoteDtoValidator()
    {
        RuleFor(x => x.VendorId).GreaterThan(0);
        RuleFor(x => x.QuotedAmount).GreaterThanOrEqualTo(0);
    }
}
