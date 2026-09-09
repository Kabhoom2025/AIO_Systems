using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Tables.Commands.PublishTable;

public record PublishTableCommand(int TableId) : IRequest<Result>, ITableScopedRequest
{
    public string RequiredPermission => PermissionCatalog.TablesWrite;
}
