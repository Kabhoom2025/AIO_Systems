using System.Text.Json.Nodes;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.GenerateAppShareToken;

public class GenerateAppShareTokenCommandHandler : IRequestHandler<GenerateAppShareTokenCommand, Result<GenerateAppShareTokenResultDto>>
{
    private static readonly HashSet<string> AllowedKinds = new(StringComparer.Ordinal) { "Guest", "Webhook" };

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public GenerateAppShareTokenCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result<GenerateAppShareTokenResultDto>> Handle(GenerateAppShareTokenCommand request, CancellationToken cancellationToken)
    {
        if (!AllowedKinds.Contains(request.Kind))
        {
            return Result<GenerateAppShareTokenResultDto>.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["kind"] = new[] { "Kind must be 'Guest' or 'Webhook'." },
            }));
        }

        if (_currentEnvironment.Stage != EnvironmentStage.Dev)
        {
            return Result<GenerateAppShareTokenResultDto>.Failure(Error.Conflict("Apps can only be edited while in the Dev sandbox stage."));
        }

        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage, cancellationToken);

        if (app is null)
        {
            return Result<GenerateAppShareTokenResultDto>.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        var token = Guid.NewGuid().ToString("N");
        var settings = JsonNode.Parse(app.SettingsJson)?.AsObject() ?? new JsonObject();
        var shared = settings["shared"]?.AsObject() ?? new JsonObject();

        if (request.Kind == "Guest")
        {
            shared["guestLinkEnabled"] = true;
            shared["guestToken"] = token;
        }
        else
        {
            shared["webhookEnabled"] = true;
            shared["webhookApiKey"] = token;
        }

        settings["shared"] = shared;

        var settingsJson = settings.ToJsonString();
        app.UpdateSettings(settingsJson);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<GenerateAppShareTokenResultDto>.Success(new GenerateAppShareTokenResultDto(token, settingsJson));
    }
}
