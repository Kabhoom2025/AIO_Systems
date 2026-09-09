using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Infrastructure.Data;

namespace ProjectFlowAI.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

/// <summary>Populated per-request from JWT claims (see TokenService for the claim shapes it reads).</summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? OrganizationId
    {
        get
        {
            var value = User?.FindFirst("organizationId")?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

    public IReadOnlyList<string> Permissions =>
        User?.FindAll("permission").Select(c => c.Value).ToList() ?? new List<string>();

    public string? IpAddress => _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}

/// <summary>Mirrors NovaERP's AuditLogWriter — persists one AuditLog row per call; wired as an
/// MVC action filter in the API layer (see ProjectFlowAI.API.Filters.AuditLogActionFilter).</summary>
public class AuditLogWriter : IAuditLogWriter
{
    private readonly ProjectFlowDbContext _ctx;

    public AuditLogWriter(ProjectFlowDbContext ctx) => _ctx = ctx;

    public async Task WriteAsync(Guid? organizationId, Guid? userId, string action, string entityType,
        string? entityId, string? metadataJson, string? ipAddress, CancellationToken cancellationToken = default)
    {
        _ctx.AuditLogs.Add(new AuditLog
        {
            OrganizationId = organizationId,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            MetadataJson = metadataJson,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        });
        await _ctx.SaveChangesAsync(cancellationToken);
    }
}
