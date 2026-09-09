using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Queries.GetAppsList;

public record GetAppsListQuery(int WorkspaceId) : IRequest<Result<List<AppSummaryDto>>>;

public record AppSummaryDto(int Id, string Name, string? Description, string? Icon, bool IsPublished, DateTime CreatedDate);
