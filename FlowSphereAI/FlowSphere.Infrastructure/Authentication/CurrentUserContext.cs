using System.Security.Claims;
using FlowSphere.Application.Common;
using Microsoft.AspNetCore.Http;

namespace FlowSphere.Infrastructure.Authentication;

/// <summary>Reads organizationId/sub/permissions claims from the current HTTP request's JWT -
/// same claim names already used by sibling services in this repo (organizationId, permissions,
/// ClaimTypes.NameIdentifier), so a token minted elsewhere resolves identically here.</summary>
public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int OrganizationId =>
        int.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst("organizationId")?.Value, out var value) ? value : 0;

    public int UserId =>
        int.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var value) ? value : 0;

    public IReadOnlyCollection<string> Permissions =>
        _httpContextAccessor.HttpContext?.User.FindFirst("permissions")?.Value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        ?? Array.Empty<string>();

    public bool HasPermission(string permission) => Permissions.Contains(permission);
}
