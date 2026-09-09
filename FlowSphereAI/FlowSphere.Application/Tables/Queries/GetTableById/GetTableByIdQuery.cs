using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Tables.Queries.GetTableById;

public record GetTableByIdQuery(int Id) : IRequest<Result<TableDetailDto>>;

public record TableDetailDto(
    int Id,
    int WorkspaceId,
    string Name,
    string? Description,
    string SchemaJson,
    bool IsPublished,
    DateTime? PublishedAt,
    DateTime CreatedDate);
