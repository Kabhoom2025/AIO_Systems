using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.AuditLogs;

public record ListAuditLogsQuery(Guid? OrganizationId = null, Guid? UserId = null, string? EntityType = null,
    DateTime? From = null, DateTime? To = null, int Page = 1, int PageSize = 50,
    string? SortBy = null, string? SortDir = "desc") : IRequest<PagedResult<AuditLogDto>>;

public class ListAuditLogsQueryHandler : IRequestHandler<ListAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListAuditLogsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<PagedResult<AuditLogDto>> Handle(ListAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.AuditLogs.AsQueryable();
        if (request.OrganizationId.HasValue) query = query.Where(a => a.OrganizationId == request.OrganizationId.Value);
        if (request.UserId.HasValue) query = query.Where(a => a.UserId == request.UserId.Value);
        if (!string.IsNullOrWhiteSpace(request.EntityType)) query = query.Where(a => a.EntityType == request.EntityType);
        if (request.From.HasValue) query = query.Where(a => a.CreatedAt >= request.From.Value);
        if (request.To.HasValue) query = query.Where(a => a.CreatedAt <= request.To.Value);

        var total = await query.CountAsync(cancellationToken);
        var sorted = query.ApplySort(request.SortBy, request.SortDir, nameof(AuditLog.CreatedAt));

        var items = await sorted.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ProjectTo<AuditLogDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>(items, total, request.Page, request.PageSize);
    }
}
