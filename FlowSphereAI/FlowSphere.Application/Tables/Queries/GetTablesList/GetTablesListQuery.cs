using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Tables.Queries.GetTablesList;

public record GetTablesListQuery(int WorkspaceId) : IRequest<Result<List<TableSummaryDto>>>;

public record TableSummaryDto(int Id, string Name, string? Description, bool IsPublished, DateTime CreatedDate);
