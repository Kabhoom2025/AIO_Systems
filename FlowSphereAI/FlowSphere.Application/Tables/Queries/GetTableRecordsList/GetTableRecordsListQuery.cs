using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Tables.Queries.GetTableRecordsList;

public record GetTableRecordsListQuery(int TableId) : IRequest<Result<List<TableRecordDto>>>;

public record TableRecordDto(int Id, string DataJson, DateTime CreatedDate);
