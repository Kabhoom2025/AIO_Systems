using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Behaviors;

/// <summary>
/// Real per-workspace permission enforcement. Only activates for requests implementing
/// IWorkspaceScopedRequest (today: the Apps/Tables write and submit commands). A user with
/// RequiredPermission in their org-global Role sails through unchanged (today's behavior for
/// Admin/HR/etc.); otherwise the user must hold a WorkspaceRoleAssignment in WorkspaceId whose
/// WorkspaceRole grants RequiredPermission, or the request is rejected before the handler runs.
/// Requires TResponse to be Result/Result&lt;T&gt;, same constraint ValidationBehavior relies on.
/// </summary>
public class WorkspacePermissionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserContext _currentUser;
    private readonly IApplicationDbContext _db;

    public WorkspacePermissionBehavior(ICurrentUserContext currentUser, IApplicationDbContext db)
    {
        _currentUser = currentUser;
        _db = db;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var (workspaceId, requiredPermission) = request switch
        {
            IWorkspaceScopedRequest w => ((int?)w.WorkspaceId, w.RequiredPermission),
            IAppScopedRequest a => (null, a.RequiredPermission),
            ITableScopedRequest t => (null, t.RequiredPermission),
            _ => ((int?)null, null),
        };

        if (requiredPermission is null)
        {
            return await next();
        }

        if (_currentUser.HasPermission(requiredPermission))
        {
            return await next();
        }

        if (workspaceId is null)
        {
            workspaceId = request switch
            {
                IAppScopedRequest a => await _db.AppDefinitions
                    .Where(x => x.Id == a.AppId).Select(x => (int?)x.WorkspaceId).FirstOrDefaultAsync(cancellationToken),
                ITableScopedRequest t => await _db.TableDefinitions
                    .Where(x => x.Id == t.TableId).Select(x => (int?)x.WorkspaceId).FirstOrDefaultAsync(cancellationToken),
                _ => null,
            };
        }

        var grants = workspaceId is null
            ? new List<string>()
            : await _db.WorkspaceRoleAssignments
                .Where(a => a.WorkspaceId == workspaceId && a.UserId == _currentUser.UserId)
                .Select(a => a.WorkspaceRole.Permissions)
                .ToListAsync(cancellationToken);

        var granted = grants.Any(permissions => permissions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(requiredPermission));

        if (granted)
        {
            return await next();
        }

        var error = Error.Unauthorized(
            $"You don't have \"{requiredPermission}\" access in this workspace.", "WorkspacePermissionDenied");

        return BuildFailureResponse(error);
    }

    private static TResponse BuildFailureResponse(Error error)
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = responseType.GetMethod(nameof(Result<object>.Failure));
            return (TResponse)failureMethod!.Invoke(null, new object[] { error })!;
        }

        throw new InvalidOperationException(
            $"{responseType.Name} is not a Result/Result<T> - WorkspacePermissionBehavior only supports Result-shaped responses.");
    }
}
