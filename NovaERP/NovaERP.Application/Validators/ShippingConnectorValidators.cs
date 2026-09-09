using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class SaveShippingConnectorDtoValidator : AbstractValidator<SaveShippingConnectorDto>
{
    private static readonly string[] ValidHttpMethods = { "GET", "POST" };
    private static readonly string[] ValidAuthTypes = { "None", "ApiKeyHeader", "BearerToken", "BasicAuth", "TokenLogin" };
    private static readonly string[] ValidRequestContentTypes = { "Json", "FormUrlEncoded", "Xml", "Text" };
    private static readonly string[] ValidResponseFormats = { "Json", "Xml" };
    private static readonly string[] ValidDirections = { "Outbound", "Inbound" };
    private static readonly string[] ValidTransforms = { "None", "KgToLb", "LbToKg", "CmToIn", "InToCm", "Uppercase", "Lowercase", "DateAppend0900", "DateFormatMDY" };

    public SaveShippingConnectorDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BaseUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.HttpMethod).Must(m => ValidHttpMethods.Contains(m))
            .WithMessage($"HttpMethod must be one of: {string.Join(", ", ValidHttpMethods)}");
        RuleFor(x => x.RequestContentType).Must(t => ValidRequestContentTypes.Contains(t))
            .WithMessage($"RequestContentType must be one of: {string.Join(", ", ValidRequestContentTypes)}");
        RuleFor(x => x.ResponseFormat).Must(f => ValidResponseFormats.Contains(f))
            .WithMessage($"ResponseFormat must be one of: {string.Join(", ", ValidResponseFormats)}");
        RuleFor(x => x.AuthType).Must(a => ValidAuthTypes.Contains(a))
            .WithMessage($"AuthType must be one of: {string.Join(", ", ValidAuthTypes)}");

        RuleForEach(x => x.FieldMappings).ChildRules(mapping =>
        {
            mapping.RuleFor(m => m.Direction).Must(d => ValidDirections.Contains(d))
                .WithMessage($"Direction must be one of: {string.Join(", ", ValidDirections)}");
            mapping.RuleFor(m => m.NovaField).NotEmpty();
            mapping.RuleFor(m => m.ExternalPath).NotEmpty();
            mapping.RuleFor(m => m.Transform).Must(t => ValidTransforms.Contains(t))
                .WithMessage($"Transform must be one of: {string.Join(", ", ValidTransforms)}");
        });
    }
}
