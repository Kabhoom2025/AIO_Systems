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

namespace ProjectFlowAI.Application.Features.Milestones;

public record CreateMilestoneCommand(Guid ProjectId, string Name, string? Description, DateTime? DueDate) : IRequest<MilestoneDto>;

public class CreateMilestoneCommandValidator : AbstractValidator<CreateMilestoneCommand>
{
    public CreateMilestoneCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class CreateMilestoneCommandHandler : IRequestHandler<CreateMilestoneCommand, MilestoneDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public CreateMilestoneCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<MilestoneDto> Handle(CreateMilestoneCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var milestone = new Milestone
        {
            ProjectId = request.ProjectId,
            Name = request.Name,
            Description = request.Description,
            DueDate = request.DueDate,
            Status = MilestoneStatus.Open
        };
        _db.Milestones.Add(milestone);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<MilestoneDto>(milestone);
    }
}

public record UpdateMilestoneCommand(Guid Id, string Name, string? Description, DateTime? DueDate, MilestoneStatus Status) : IRequest<MilestoneDto>;

public class UpdateMilestoneCommandValidator : AbstractValidator<UpdateMilestoneCommand>
{
    public UpdateMilestoneCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class UpdateMilestoneCommandHandler : IRequestHandler<UpdateMilestoneCommand, MilestoneDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public UpdateMilestoneCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<MilestoneDto> Handle(UpdateMilestoneCommand request, CancellationToken cancellationToken)
    {
        var milestone = await _db.Milestones.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Milestone", request.Id);

        milestone.Name = request.Name;
        milestone.Description = request.Description;
        milestone.DueDate = request.DueDate;
        milestone.Status = request.Status;
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<MilestoneDto>(milestone);
    }
}

public record DeleteMilestoneCommand(Guid Id) : IRequest<Unit>;

public class DeleteMilestoneCommandValidator : AbstractValidator<DeleteMilestoneCommand>
{
    public DeleteMilestoneCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteMilestoneCommandHandler : IRequestHandler<DeleteMilestoneCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public DeleteMilestoneCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteMilestoneCommand request, CancellationToken cancellationToken)
    {
        var milestone = await _db.Milestones.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Milestone", request.Id);

        _db.Milestones.Remove(milestone);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record ListMilestonesQuery(Guid ProjectId, int Page = 1, int PageSize = 50,
    string? SortBy = null, string? SortDir = "asc") : IRequest<PagedResult<MilestoneDto>>;

public class ListMilestonesQueryHandler : IRequestHandler<ListMilestonesQuery, PagedResult<MilestoneDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListMilestonesQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<PagedResult<MilestoneDto>> Handle(ListMilestonesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Milestones.Where(m => m.ProjectId == request.ProjectId);
        var total = await query.CountAsync(cancellationToken);
        var sorted = query.ApplySort(request.SortBy, request.SortDir, nameof(Milestone.DueDate));

        var items = await sorted.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ProjectTo<MilestoneDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);

        return new PagedResult<MilestoneDto>(items, total, request.Page, request.PageSize);
    }
}
