using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Queries.GetDeploymentLog;

public class GetDeploymentLogQueryHandler : IRequestHandler<GetDeploymentLogQuery, Result<List<DeploymentLogEntryDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetDeploymentLogQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<DeploymentLogEntryDto>>> Handle(GetDeploymentLogQuery request, CancellationToken cancellationToken)
    {
        // DeploymentLogEntries is ITenantScoped, so this is already implicitly filtered to the
        // current organization by the global query filter.
        var query = _db.DeploymentLogEntries.AsQueryable();

        if (request.Stage is EnvironmentStage stage)
        {
            query = query.Where(d => d.FromStage == stage || d.ToStage == stage);
        }

        var rows = await query
            .OrderByDescending(d => d.CreatedDate)
            .Select(d => new
            {
                d.Id, d.AppName, d.FromStage, d.ToStage, d.Action, d.CreatedDate,
                PerformedByUserName = d.PerformedByUser.Name,
            })
            .ToListAsync(cancellationToken);

        var dtos = rows
            .Select(d => new DeploymentLogEntryDto(
                d.Id, d.AppName, d.FromStage.ToString(), d.ToStage.ToString(), d.Action.ToString(),
                d.PerformedByUserName, d.CreatedDate))
            .ToList();

        return Result<List<DeploymentLogEntryDto>>.Success(dtos);
    }
}
