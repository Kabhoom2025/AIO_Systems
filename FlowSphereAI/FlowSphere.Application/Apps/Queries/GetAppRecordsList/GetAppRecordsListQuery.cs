using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Queries.GetAppRecordsList;

public record GetAppRecordsListQuery(int AppId) : IRequest<Result<List<AppRecordDto>>>;

public record AppRecordDto(int Id, string DataJson, DateTime CreatedDate);
