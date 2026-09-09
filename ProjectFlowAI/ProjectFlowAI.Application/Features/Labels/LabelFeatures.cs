using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Labels;

public record CreateLabelCommand(Guid ProjectId, string Name, string ColorHex) : IRequest<LabelDto>;

public class CreateLabelCommandValidator : AbstractValidator<CreateLabelCommand>
{
    public CreateLabelCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ColorHex).NotEmpty().Matches("^#[0-9A-Fa-f]{6}$").WithMessage("ColorHex must look like #RRGGBB.");
    }
}

public class CreateLabelCommandHandler : IRequestHandler<CreateLabelCommand, LabelDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public CreateLabelCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<LabelDto> Handle(CreateLabelCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var label = new Label { ProjectId = request.ProjectId, Name = request.Name, ColorHex = request.ColorHex };
        _db.Labels.Add(label);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<LabelDto>(label);
    }
}

public record DeleteLabelCommand(Guid Id) : IRequest<Unit>;

public class DeleteLabelCommandValidator : AbstractValidator<DeleteLabelCommand>
{
    public DeleteLabelCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteLabelCommandHandler : IRequestHandler<DeleteLabelCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public DeleteLabelCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteLabelCommand request, CancellationToken cancellationToken)
    {
        var label = await _db.Labels.FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Label", request.Id);

        _db.Labels.Remove(label);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record ListLabelsQuery(Guid ProjectId) : IRequest<IReadOnlyList<LabelDto>>;

public class ListLabelsQueryHandler : IRequestHandler<ListLabelsQuery, IReadOnlyList<LabelDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListLabelsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<IReadOnlyList<LabelDto>> Handle(ListLabelsQuery request, CancellationToken cancellationToken) =>
        await _db.Labels.Where(l => l.ProjectId == request.ProjectId).OrderBy(l => l.Name)
            .ProjectTo<LabelDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
}
