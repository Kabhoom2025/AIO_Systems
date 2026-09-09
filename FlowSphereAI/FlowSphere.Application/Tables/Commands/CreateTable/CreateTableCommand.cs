using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Tables.Commands.CreateTable;

public record CreateTableCommand(int WorkspaceId, string Name, string? Description) : IRequest<Result<int>>, IWorkspaceScopedRequest
{
    public string RequiredPermission => PermissionCatalog.TablesWrite;
}
