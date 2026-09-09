using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Queries.GetAppRecordComments;

public record GetAppRecordCommentsQuery(int AppId, int AppRecordId) : IRequest<Result<List<AppRecordCommentDto>>>;

public record AppRecordCommentDto(int Id, string UserName, string Text, DateTime CreatedDate);
