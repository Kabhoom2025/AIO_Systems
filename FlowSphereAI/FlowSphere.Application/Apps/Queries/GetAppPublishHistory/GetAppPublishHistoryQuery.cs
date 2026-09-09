using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Queries.GetAppPublishHistory;

public record GetAppPublishHistoryQuery(int AppId) : IRequest<Result<List<AppPublishHistoryEntryDto>>>;

public record AppPublishHistoryEntryDto(
    int Id,
    string? Comment,
    string PublishedByUserName,
    DateTime CreatedDate);
