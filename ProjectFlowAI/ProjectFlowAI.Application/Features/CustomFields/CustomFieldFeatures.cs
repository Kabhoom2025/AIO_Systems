using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.CustomFields;

public record CreateCustomFieldDefinitionCommand(Guid ProjectId, string Name, CustomFieldType FieldType,
    string? OptionsJson, bool IsRequired) : IRequest<CustomFieldDefinitionDto>;

public class CreateCustomFieldDefinitionCommandValidator : AbstractValidator<CreateCustomFieldDefinitionCommand>
{
    public CreateCustomFieldDefinitionCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.FieldType).IsInEnum();
        RuleFor(x => x.OptionsJson).NotEmpty().When(x => x.FieldType == CustomFieldType.Dropdown)
            .WithMessage("OptionsJson is required when FieldType is Dropdown.");
    }
}

public class CreateCustomFieldDefinitionCommandHandler : IRequestHandler<CreateCustomFieldDefinitionCommand, CustomFieldDefinitionDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public CreateCustomFieldDefinitionCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<CustomFieldDefinitionDto> Handle(CreateCustomFieldDefinitionCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var def = new CustomFieldDefinition
        {
            ProjectId = request.ProjectId,
            Name = request.Name,
            FieldType = request.FieldType,
            OptionsJson = request.FieldType == CustomFieldType.Dropdown ? request.OptionsJson : null,
            IsRequired = request.IsRequired
        };
        _db.CustomFieldDefinitions.Add(def);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CustomFieldDefinitionDto>(def);
    }
}

public record DeleteCustomFieldDefinitionCommand(Guid Id) : IRequest<Unit>;

public class DeleteCustomFieldDefinitionCommandValidator : AbstractValidator<DeleteCustomFieldDefinitionCommand>
{
    public DeleteCustomFieldDefinitionCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteCustomFieldDefinitionCommandHandler : IRequestHandler<DeleteCustomFieldDefinitionCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public DeleteCustomFieldDefinitionCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteCustomFieldDefinitionCommand request, CancellationToken cancellationToken)
    {
        var def = await _db.CustomFieldDefinitions.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("CustomFieldDefinition", request.Id);

        _db.CustomFieldDefinitions.Remove(def);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record SetCustomFieldValueCommand(Guid WorkItemId, Guid CustomFieldDefinitionId, string ValueJson) : IRequest<CustomFieldValueDto>;

public class SetCustomFieldValueCommandValidator : AbstractValidator<SetCustomFieldValueCommand>
{
    public SetCustomFieldValueCommandValidator()
    {
        RuleFor(x => x.WorkItemId).NotEmpty();
        RuleFor(x => x.CustomFieldDefinitionId).NotEmpty();
        RuleFor(x => x.ValueJson).NotNull();
    }
}

public class SetCustomFieldValueCommandHandler : IRequestHandler<SetCustomFieldValueCommand, CustomFieldValueDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public SetCustomFieldValueCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<CustomFieldValueDto> Handle(SetCustomFieldValueCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.WorkItems.AnyAsync(w => w.Id == request.WorkItemId, cancellationToken))
            throw new NotFoundException("WorkItem", request.WorkItemId);
        if (!await _db.CustomFieldDefinitions.AnyAsync(d => d.Id == request.CustomFieldDefinitionId, cancellationToken))
            throw new NotFoundException("CustomFieldDefinition", request.CustomFieldDefinitionId);

        var value = await _db.CustomFieldValues.FirstOrDefaultAsync(
            v => v.WorkItemId == request.WorkItemId && v.CustomFieldDefinitionId == request.CustomFieldDefinitionId, cancellationToken);

        if (value == null)
        {
            value = new CustomFieldValue { WorkItemId = request.WorkItemId, CustomFieldDefinitionId = request.CustomFieldDefinitionId, ValueJson = request.ValueJson };
            _db.CustomFieldValues.Add(value);
        }
        else
        {
            value.ValueJson = request.ValueJson;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CustomFieldValueDto>(value);
    }
}

public record ListCustomFieldDefinitionsQuery(Guid ProjectId) : IRequest<IReadOnlyList<CustomFieldDefinitionDto>>;

public class ListCustomFieldDefinitionsQueryHandler : IRequestHandler<ListCustomFieldDefinitionsQuery, IReadOnlyList<CustomFieldDefinitionDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListCustomFieldDefinitionsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<IReadOnlyList<CustomFieldDefinitionDto>> Handle(ListCustomFieldDefinitionsQuery request, CancellationToken cancellationToken) =>
        await _db.CustomFieldDefinitions.Where(d => d.ProjectId == request.ProjectId).OrderBy(d => d.Name)
            .ProjectTo<CustomFieldDefinitionDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
}
