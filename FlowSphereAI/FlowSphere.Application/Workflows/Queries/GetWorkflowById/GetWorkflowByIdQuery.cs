using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workflows.Queries.GetWorkflowById;

public record GetWorkflowByIdQuery(int Id) : IRequest<Result<WorkflowDetailDto>>;

public record WorkflowDetailDto(int Id, string Name, string? Description, bool IsEnabled, DateTime CreatedDate);
