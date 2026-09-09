using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace FlowSphere.Application.Dashboard.Queries.GetDashboardKpis;

public class GetDashboardKpisQueryHandler : IRequestHandler<GetDashboardKpisQuery, Result<DashboardKpisDto>>
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15),
    };

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly IDistributedCache _cache;

    public GetDashboardKpisQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser, IDistributedCache cache)
    {
        _db = db;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<DashboardKpisDto>> Handle(GetDashboardKpisQuery request, CancellationToken cancellationToken)
    {
        // Short TTL distributed cache (Redis-backed in production) - the dashboard polls this on
        // every page load/refresh, and a 15s staleness window is an easy trade against hitting
        // Postgres with 4 COUNT queries every time, shared across however many API instances.
        var cacheKey = $"dashboard-kpis:{_currentUser.OrganizationId}";
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return Result<DashboardKpisDto>.Success(JsonSerializer.Deserialize<DashboardKpisDto>(cached)!);
        }

        var organization = await _db.Organizations
            .FirstOrDefaultAsync(o => o.Id == _currentUser.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result<DashboardKpisDto>.Failure(Error.NotFound("Organization not found."));
        }

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var executionsThisMonth = await _db.WorkflowExecutions.CountAsync(e => e.CreatedDate >= monthStart, cancellationToken);
        var totalWorkflows = await _db.WorkflowDefinitions.CountAsync(cancellationToken);
        var succeededCount = await _db.WorkflowExecutions.CountAsync(e => e.Status == ExecutionStatus.Succeeded, cancellationToken);
        var failedCount = await _db.WorkflowExecutions.CountAsync(e => e.Status == ExecutionStatus.Failed, cancellationToken);

        var dto = new DashboardKpisDto(
            organization.PlanTier,
            organization.MonthlyExecutionQuota,
            executionsThisMonth,
            totalWorkflows,
            succeededCount,
            failedCount);

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), CacheOptions, cancellationToken);

        return Result<DashboardKpisDto>.Success(dto);
    }
}
