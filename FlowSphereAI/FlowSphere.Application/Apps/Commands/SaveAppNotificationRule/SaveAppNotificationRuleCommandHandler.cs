using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.SaveAppNotificationRule;

public class SaveAppNotificationRuleCommandHandler : IRequestHandler<SaveAppNotificationRuleCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public SaveAppNotificationRuleCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result<int>> Handle(SaveAppNotificationRuleCommand request, CancellationToken cancellationToken)
    {
        if (_currentEnvironment.Stage != EnvironmentStage.Dev)
        {
            return Result<int>.Failure(Error.Conflict("Apps can only be edited while in the Dev sandbox stage."));
        }

        if (!IsValidConditionsJson(request.ConditionsJson))
        {
            return Result<int>.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["conditionsJson"] = new[] { "Conditions must be a JSON array." },
            }));
        }

        var appExists = await _db.AppDefinitions
            .AnyAsync(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage, cancellationToken);
        if (!appExists)
        {
            return Result<int>.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        var rule = request.Id is null
            ? new AppNotificationRule { OrganizationId = _currentUser.OrganizationId, AppId = request.AppId }
            : await _db.AppNotificationRules.FirstOrDefaultAsync(r => r.Id == request.Id && r.AppId == request.AppId, cancellationToken);

        if (rule is null)
        {
            return Result<int>.Failure(Error.NotFound($"Notification rule {request.Id} was not found."));
        }

        rule.Name = request.Name;
        rule.Event = request.Event;
        rule.Level = request.Level;
        rule.StepKey = request.StepKey;
        rule.ActionKey = request.ActionKey;
        rule.Channel = request.Channel;
        rule.SenderEmail = request.SenderEmail;
        rule.RecipientType = request.RecipientType;
        rule.RecipientRoleIdsJson = JsonSerializer.Serialize(request.RecipientRoleIds);
        rule.RecipientUserIdsJson = JsonSerializer.Serialize(request.RecipientUserIds);
        rule.RecipientDataFieldKey = request.RecipientDataFieldKey;
        rule.CcUserIdsJson = JsonSerializer.Serialize(request.CcUserIds);
        rule.Subject = request.Subject;
        rule.Body = request.Body;
        rule.ConditionsJson = request.ConditionsJson;
        rule.SendAttachments = request.SendAttachments;
        rule.SendReports = request.SendReports;
        rule.IsEnabled = request.IsEnabled;

        if (request.Id is null)
        {
            _db.AppNotificationRules.Add(rule);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(rule.Id);
    }

    private static bool IsValidConditionsJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
