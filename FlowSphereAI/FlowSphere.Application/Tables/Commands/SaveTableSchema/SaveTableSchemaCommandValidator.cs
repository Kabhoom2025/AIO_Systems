using FluentValidation;

namespace FlowSphere.Application.Tables.Commands.SaveTableSchema;

public class SaveTableSchemaCommandValidator : AbstractValidator<SaveTableSchemaCommand>
{
    public SaveTableSchemaCommandValidator()
    {
        RuleFor(x => x.TableId).GreaterThan(0);
        RuleFor(x => x.SchemaJson).NotEmpty();
    }
}
