using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Queries.GetAppNotificationRules;

public class GetAppNotificationRulesQueryHandler : IRequestHandler<GetAppNotificationRulesQuery, Result<List<AppNotificationRuleDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetAppNotificationRulesQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<AppNotificationRuleDto>>> Handle(GetAppNotificationRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _db.AppNotificationRules
            .Where(r => r.AppId == request.AppId && r.OrganizationId == _currentUser.OrganizationId)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var items = rules.Select(r => new AppNotificationRuleDto(
            r.Id, r.Name, r.Event, r.Level, r.StepKey, r.ActionKey,
            r.Channel, r.SenderEmail, r.RecipientType,
            ParseIntArray(r.RecipientRoleIdsJson), ParseIntArray(r.RecipientUserIdsJson), r.RecipientDataFieldKey, ParseIntArray(r.CcUserIdsJson),
            r.Subject, r.Body, r.ConditionsJson, r.SendAttachments, r.SendReports, r.IsEnabled)).ToList();

        return Result<List<AppNotificationRuleDto>>.Success(items);
    }

    private static List<int> ParseIntArray(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }
        catch (JsonException)
        {
            return new List<int>();
        }
    }
}
