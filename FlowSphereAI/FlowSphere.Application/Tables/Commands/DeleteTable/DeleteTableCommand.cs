using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Tables.Commands.DeleteTable;

public record DeleteTableCommand(int TableId) : IRequest<Result>, ITableScopedRequest
{
    public string RequiredPermission => PermissionCatalog.TablesWrite;
}
