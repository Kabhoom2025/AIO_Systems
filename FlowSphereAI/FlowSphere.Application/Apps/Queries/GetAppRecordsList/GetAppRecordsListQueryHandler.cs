using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Queries.GetAppRecordsList;

public class GetAppRecordsListQueryHandler : IRequestHandler<GetAppRecordsListQuery, Result<List<AppRecordDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public GetAppRecordsListQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result<List<AppRecordDto>>> Handle(GetAppRecordsListQuery request, CancellationToken cancellationToken)
    {
        // AppRecords itself is IEnvironmentScoped, so the query below is already implicitly
        // filtered to the current stage by the global query filter - only the AppDefinition
        // existence check needs an explicit predicate.
        var app = await _db.AppDefinitions
            .Where(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage)
            .Select(a => new { a.Id, a.SettingsJson })
            .FirstOrDefaultAsync(cancellationToken);

        if (app is null)
        {
            return Result<List<AppRecordDto>>.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        // The controller route allows any authenticated org member to call this endpoint (not
        // gated by [Authorize(Policy=AppsRead)]) so an App Review reviewer without apps.read can
        // still reach the handler - this is the real access check: normal apps.read holders pass
        // immediately, everyone else needs to be an explicitly configured reviewer.
        if (!_currentUser.HasPermission(PermissionCatalog.AppsRead) && !await IsAppReviewerAsync(app.SettingsJson, cancellationToken))
        {
            return Result<List<AppRecordDto>>.Failure(Error.Unauthorized("You do not have permission to view this app's records."));
        }

        var items = await _db.AppRecords
            .Where(r => r.AppDefinitionId == request.AppId)
            .OrderByDescending(r => r.CreatedDate)
            .Select(r => new AppRecordDto(r.Id, r.DataJson, r.CreatedDate))
            .ToListAsync(cancellationToken);

        return Result<List<AppRecordDto>>.Success(items);
    }

    private async Task<bool> IsAppReviewerAsync(string settingsJson, CancellationToken cancellationToken)
    {
        try
        {
            using var doc = JsonDocument.Parse(settingsJson);
            if (!doc.RootElement.TryGetProperty("attributes", out var attributes)
                || !attributes.TryGetProperty("appReview", out var appReview)
                || !appReview.TryGetProperty("enabled", out var enabledEl) || enabledEl.ValueKind != JsonValueKind.True)
            {
                return false;
            }

            if (appReview.TryGetProperty("userIds", out var userIdsEl) && userIdsEl.ValueKind == JsonValueKind.Array
                && userIdsEl.EnumerateArray().Any(e => e.ValueKind == JsonValueKind.Number && e.GetInt32() == _currentUser.UserId))
            {
                return true;
            }

            if (appReview.TryGetProperty("roleIds", out var roleIdsEl) && roleIdsEl.ValueKind == JsonValueKind.Array && roleIdsEl.GetArrayLength() > 0)
            {
                var roleIds = roleIdsEl.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.Number).Select(e => e.GetInt32()).ToHashSet();
                var myRoleId = await _db.Users.Where(u => u.Id == _currentUser.UserId).Select(u => u.RoleId).FirstOrDefaultAsync(cancellationToken);
                return myRoleId is not null && roleIds.Contains(myRoleId.Value);
            }

            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
