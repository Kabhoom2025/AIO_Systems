using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.SaveAppBusinessRules;

public class SaveAppBusinessRulesCommandHandler : IRequestHandler<SaveAppBusinessRulesCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public SaveAppBusinessRulesCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result> Handle(SaveAppBusinessRulesCommand request, CancellationToken cancellationToken)
    {
        if (_currentEnvironment.Stage != EnvironmentStage.Dev)
        {
            return Result.Failure(Error.Conflict("Apps can only be edited while in the Dev sandbox stage."));
        }

        if (!IsValidBusinessRules(request.BusinessRulesJson))
        {
            return Result.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["businessRulesJson"] = new[] { "The rules must be a JSON array of rule objects, each with an 'id', 'name', 'kind', 'event', and 'branches' array." },
            }));
        }

        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage, cancellationToken);

        if (app is null)
        {
            return Result.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        app.UpdateBusinessRules(request.BusinessRulesJson);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool IsValidBusinessRules(string businessRulesJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(businessRulesJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (var rule in doc.RootElement.EnumerateArray())
            {
                if (rule.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                if (!rule.TryGetProperty("id", out _) || !rule.TryGetProperty("name", out _))
                {
                    return false;
                }

                if (!rule.TryGetProperty("kind", out var kind) || kind.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                if (!rule.TryGetProperty("event", out var evt) || evt.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                if (!rule.TryGetProperty("branches", out var branches) || branches.ValueKind != JsonValueKind.Array)
                {
                    return false;
                }
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
