using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using MediatR;

namespace FlowSphere.Application.Tables.Commands.SaveTableSchema;

public record SaveTableSchemaCommand(int TableId, string SchemaJson) : IRequest<Result>, ITableScopedRequest
{
    public string RequiredPermission => PermissionCatalog.TablesWrite;
}
