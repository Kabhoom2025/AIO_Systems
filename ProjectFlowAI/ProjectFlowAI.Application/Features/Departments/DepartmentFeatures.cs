using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Departments;

public record CreateDepartmentCommand(Guid OrganizationId, string Name, string? Description, Guid? ParentDepartmentId) : IRequest<DepartmentDto>;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public CreateDepartmentCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Organizations.AnyAsync(o => o.Id == request.OrganizationId, cancellationToken))
            throw new NotFoundException("Organization", request.OrganizationId);

        var dept = new Department
        {
            OrganizationId = request.OrganizationId,
            Name = request.Name,
            Description = request.Description,
            ParentDepartmentId = request.ParentDepartmentId
        };
        _db.Departments.Add(dept);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<DepartmentDto>(dept);
    }
}

public record UpdateDepartmentCommand(Guid Id, string Name, string? Description, Guid? ParentDepartmentId) : IRequest<DepartmentDto>;

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public UpdateDepartmentCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<DepartmentDto> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var dept = await _db.Departments.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Department", request.Id);

        dept.Name = request.Name;
        dept.Description = request.Description;
        dept.ParentDepartmentId = request.ParentDepartmentId;
        dept.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<DepartmentDto>(dept);
    }
}

public record DeleteDepartmentCommand(Guid Id) : IRequest<Unit>;

public class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public DeleteDepartmentCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var dept = await _db.Departments.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Department", request.Id);

        _db.Departments.Remove(dept);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record ListDepartmentsQuery(Guid OrganizationId, int Page = 1, int PageSize = 20,
    string? SortBy = null, string? SortDir = "asc", string? Search = null) : IRequest<PagedResult<DepartmentDto>>;

public class ListDepartmentsQueryHandler : IRequestHandler<ListDepartmentsQuery, PagedResult<DepartmentDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListDepartmentsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<PagedResult<DepartmentDto>> Handle(ListDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Departments.Where(d => d.OrganizationId == request.OrganizationId);
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(d => d.Name.Contains(request.Search));

        var total = await query.CountAsync(cancellationToken);
        query = query.ApplySort(request.SortBy, request.SortDir, nameof(Department.Name));

        var items = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ProjectTo<DepartmentDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);

        return new PagedResult<DepartmentDto>(items, total, request.Page, request.PageSize);
    }
}
