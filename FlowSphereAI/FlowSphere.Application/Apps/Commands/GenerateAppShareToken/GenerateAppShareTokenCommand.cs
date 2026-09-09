using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Commands.GenerateAppShareToken;

/// <summary>Kind is "Guest" or "Webhook" - generates a new opaque token server-side (never
/// client-supplied) and writes it into SettingsJson.shared.{guestToken|webhookApiKey}, enabling
/// the corresponding flag. Returns the plaintext token once - the standard "shown once" secret UX,
/// same as an API key generation flow.</summary>
public record GenerateAppShareTokenCommand(int AppId, string Kind) : IRequest<Result<GenerateAppShareTokenResultDto>>, IAppScopedRequest
{
    public string RequiredPermission => PermissionCatalog.AppsWrite;
}

public record GenerateAppShareTokenResultDto(string Token, string SettingsJson);
